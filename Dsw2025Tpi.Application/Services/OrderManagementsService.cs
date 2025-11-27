using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Application.Services;

// Servicio de aplicación para la gestión de órdenes.
// Implementa IOrderManagementsService.
public class OrderManagementsService : IOrderManagementsService
{
    private readonly IRepository _repository;
    private readonly ILogger<OrderManagementsService> _logger;

    // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
    public OrderManagementsService(IRepository repository, ILogger<OrderManagementsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // Caso de uso: Crear una nueva orden.
    public async Task<OrderModelDto.OrderResponse> AddOrder(OrderModelDto.OrderRequest request)
    {
        _logger.LogInformation("Creando orden para CustomerId={CustomerId} con {Count} productos",
            request.customerId, request.Products?.Count ?? 0);

        // Validar que el cliente exista
        var exist = await _repository.First<Customer>(c => c.Id == request.customerId);
        if (exist == null)
        {
            _logger.LogWarning("Creación de orden rechazada: Customer {CustomerId} no existe", request.customerId);
            throw new NotExistException($"No existe el Customer con id {request.customerId}");
        }

        // Validar direcciones de envío/facturación
        if (IsAddressValid(request))
        {
            _logger.LogWarning("Direcciones inválidas para CustomerId={CustomerId}", request.customerId);
            throw new ArgumentException("Valores de Dirección no válidos");
        }

        // Validar que haya productos en la orden
        if (request.Products.Count == 0)
        {
            _logger.LogWarning("Orden vacía para CustomerId={CustomerId}", request.customerId);
            throw new OrderEmptyException("No tiene ningún producto cargado al carrito");
        }

        // Construir la orden y descontar stock
        var order = await CreateOrderAsync(request);

        // Guardar en base de datos
        await _repository.Add(order);
        _logger.LogInformation("Orden {OrderId} creada correctamente para CustomerId={CustomerId}", order.Id, request.customerId);

        // Mapear ítems de la orden a DTOs de respuesta
        var orderItemResponses = order.OrderItems.Select(oi => new OrderItemsModelDto.OrderItemResponse(
            oi.ProductID,
            oi.Product.Name,
            oi.Quantity,
            oi.UnitPrice
        )).ToList();

        // Devolver DTO de respuesta con la orden completa
        return new OrderModelDto.OrderResponse(
            order.CustomerId,
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            orderItemResponses,
            order.OrderStatus
        );
    }

    // Valida que direcciones no estén vacías (true = inválido)
    private static bool IsAddressValid(OrderModelDto.OrderRequest request) =>
        string.IsNullOrEmpty(request.shippingAddress) ||
        string.IsNullOrEmpty(request.billingAddress);

    // Construye y retorna una nueva orden con ítems y stock actualizado.
    private async Task<Order> CreateOrderAsync(OrderModelDto.OrderRequest request)
    {
        // Recuperar el cliente (ya validado en AddOrder)
        var customer = await _repository.First<Customer>(c => c.Id == request.customerId);

        // Crear la orden y cumplir con el 'required Customer'
        var order = new Order(request.customerId, request.shippingAddress, request.billingAddress)
        {
            Customer = customer! // Asignación obligatoria para propiedad 'required'
        };

        // Validar todos los productos antes de modificar stock
        var productosValidados = new List<(ProductModelDto.ProductRequest req, Product producto)>();

        foreach (var product in request.Products)
        {
            var producto = await _repository.First<Product>(p => p.Id == product.id);
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado: {ProductId}", product.id);
                throw new NotExistException($"No existe el Producto con nombre {product.name}");
            }

            if (!producto.IsActive)
            {
                _logger.LogWarning("Producto inactivo: {ProductId} - {ProductName}", producto.Id, producto.Name);
                throw new InvalidOperationException($"El producto '{producto.Name}' no está activo.");
            }

            if (producto.StockQuantity < product.quantity)
            {
                _logger.LogWarning("Stock insuficiente para ProductoId={ProductId} solicitado={Qty} disponible={Stock}",
                    producto.Id, product.quantity, producto.StockQuantity);
                throw new StockInsufficientException($"Stock insuficiente del Producto: {producto.Name}");
            }

            productosValidados.Add((product, producto));
            _logger.LogInformation("Producto validado: {ProductId} - {ProductName}", producto.Id, producto.Name);
        }

        // Guardar productos ya validados
        foreach (var item in productosValidados)
        {
            var product = item.req;
            var producto = item.producto;

            var orderItem = new OrderItem(
                producto.Id,
                order.Id,
                product.quantity,
                producto.CurrentUnitPrice
            )
            {
                Product = producto,
                Order = order
            };

            order.OrderItems.Add(orderItem);
            order.TotalAmount += producto.CurrentUnitPrice * product.quantity;

            producto.StockQuantity -= product.quantity;
            await _repository.Update(producto);

            _logger.LogInformation("Item agregado a orden: ProductId={ProductId}, Qty={Qty}",
                producto.Id, product.quantity);
        }

        _logger.LogInformation("Orden creada correctamente para CustomerId={CustomerId} con {ItemCount} ítems. Total=${Total}",
            order.CustomerId, order.OrderItems.Count, order.TotalAmount);

        return order;
    }
    // Obtener orden por su identificador
    public async Task<Order?> GetOrderById(Guid id) =>
        await _repository.GetById<Order>(id);

    // Cambiar estado de una orden existente
    public async Task<OrderModelDto.OrderResponse> UpdateOrderStatus(Guid id, OrderModelDto.OrderStatusUpdateRequest request)
    {
        // Recuperar orden
        var order = await _repository.GetById<Order>(id);
        if (order == null)
        {
            _logger.LogWarning("UpdateStatus rechazado: OrderId {OrderId} no existe", id);
            throw new NotExistException($"No existe la orden con id: {id}");
        }

        // Validar que el estado sea uno de los definidos en el enum OrderStatus
        if (!Enum.IsDefined(typeof(OrderStatus), request.OrderStatus))
        {
            _logger.LogWarning("Estado de orden inválido: {OrderStatus}", request.OrderStatus);
            throw new NotExistOrderStatusException($"El estado de la orden {request.OrderStatus} no es válido");
        }

        // Actualizar estado
        order.OrderStatus = request.OrderStatus;
        await _repository.Update(order);

        _logger.LogInformation("Orden {OrderId} actualizada a estado {Status}", id, order.OrderStatus);

        // Mapear ítems a DTOs de respuesta
        var orderItemResponses = order.OrderItems.Select(oi => new OrderItemsModelDto.OrderItemResponse(
            oi.ProductID,
            oi.Product.Name,
            oi.Quantity,
            oi.UnitPrice
        )).ToList();

        return new OrderModelDto.OrderResponse(
            order.CustomerId,
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            orderItemResponses,
            order.OrderStatus
        );
    }

    // Filtrar órdenes por estado, cliente y otros criterios
    public async Task<List<OrderModelDto.OrderResponse>> GetAllOrdersFilters(OrderModelDto.OrderFilterRequest request)
    {
        _logger.LogInformation("Listando órdenes con filtros: estado={OrderStatus}, customerId={CustomerId}, página={Page}, tamaño={Size}",
            request.OrderStatus, request.CustomerId, request.pageNumber, request.pagesize);

        // Construir predicado dinámico para filtrar resultados
        Expression<Func<Order, bool>> predicate = o =>
            (!request.OrderStatus.HasValue || o.OrderStatus == request.OrderStatus.Value) &&
            (!request.CustomerId.HasValue || o.CustomerId == request.CustomerId.Value);

        // Obtener lista completa filtrada con include
        var orders = await _repository.GetFiltered<Order>(predicate, "OrderItems");

        if (orders == null || !orders.Any())
        {
            _logger.LogInformation("No se encontraron órdenes con los filtros aplicados.");
            return new List<OrderModelDto.OrderResponse>();
        }

        // Aplicar paginación solo si ambos valores están presentes
        if (request.pageNumber.HasValue && request.pagesize.HasValue)
        {
            int skip = (request.pageNumber.Value - 1) * request.pagesize.Value;
            _logger.LogInformation("Aplicando paginación: skip={Skip}, take={Take}", skip, request.pagesize.Value);
            orders = orders.Skip(skip).Take(request.pagesize.Value).ToList();
        }

        // Convertir órdenes a DTOs de respuesta
        var response = orders.Select(order => new OrderModelDto.OrderResponse(
            order.CustomerId,
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            order.OrderItems.Select(oi => new OrderItemsModelDto.OrderItemResponse(
                oi.ProductID,
                oi.Product?.Name ?? string.Empty,
                oi.Quantity,
                oi.UnitPrice
            )).ToList(),
            order.OrderStatus
        )).ToList();

        _logger.LogInformation("Conversión a DTO completada. Total de respuestas: {ResponseCount}", response.Count);

        return response;
    }

    public async Task<OrderModelDto.OrderListResponse> GetAllOrdersFilter(OrderModelDto.OrderFilterRequest request)
    {
        _logger.LogInformation("Listando órdenes con filtros: estado={OrderStatus}, customerId={CustomerId}, página={Page}, tamaño={Size}",
            request.OrderStatus, request.CustomerId, request.pageNumber, request.pagesize);

        // Construir predicado dinámico para filtrar resultados
        Expression<Func<Order, bool>> predicate = o =>
            (!request.OrderStatus.HasValue || o.OrderStatus == request.OrderStatus.Value) &&
            (!request.CustomerId.HasValue || o.CustomerId == request.CustomerId.Value);

        // Obtener lista completa filtrada con include
        var allOrders = await _repository.GetFiltered<Order>(predicate, "OrderItems");

        if (allOrders == null || !allOrders.Any())
        {
            _logger.LogInformation("No se encontraron órdenes con los filtros aplicados.");
            return new OrderModelDto.OrderListResponse(0, new List<OrderModelDto.OrderResponse>());
        }

        var total = allOrders.Count();

        // Aplicar paginación solo si ambos valores están presentes
        if (request.pageNumber.HasValue && request.pagesize.HasValue)
        {
            int skip = (request.pageNumber.Value - 1) * request.pagesize.Value;
            _logger.LogInformation("Aplicando paginación: skip={Skip}, take={Take}", skip, request.pagesize.Value);
            allOrders = allOrders.Skip(skip).Take(request.pagesize.Value).ToList();
        }

        // Convertir órdenes a DTOs de respuesta
        var response = allOrders.Select(order => new OrderModelDto.OrderResponse(
            order.CustomerId,
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            order.OrderItems.Select(oi => new OrderItemsModelDto.OrderItemResponse(
                oi.ProductID,
                oi.Product?.Name ?? string.Empty,
                oi.Quantity,
                oi.UnitPrice
            )).ToList(),
            order.OrderStatus
        )).ToList();

        _logger.LogInformation("Conversión a DTO completada. Total de respuestas: {ResponseCount}", response.Count);

        return new OrderModelDto.OrderListResponse(total, response);
    }



    public async Task<int> GetOrderCountAsync()
    {
        var allOrders = await _repository.GetFiltered<Order>(o => true);
        return (allOrders ?? new List<Order>()).Count();

    }

    public async Task<OrderModelDto.OrderReadListResponse> GetAllOrdersWithCustomerName(OrderModelDto.OrderFilterRequestName request)
    {
        _logger.LogInformation("Listando órdenes con nombre de cliente: estado={OrderStatus}, cliente={CustomerId}, página={Page}, tamaño={Size}",
            request.OrderStatus, request.CustomerName, request.pageNumber, request.pagesize);

        Expression<Func<Order, bool>> predicate = o =>
            (!request.OrderStatus.HasValue || o.OrderStatus == request.OrderStatus.Value) &&
            (!string.IsNullOrEmpty(request.CustomerName)
            ? o.Customer.Name.ToLower().Contains(request.CustomerName.ToLower())
            : true);

        var allOrders = await _repository.GetFiltered<Order>(predicate, "OrderItems.Product", "Customer");

        if (allOrders == null || !allOrders.Any())
        {
            return new OrderModelDto.OrderReadListResponse(0, new List<OrderModelDto.OrderReadResponse>());
        }

        var total = allOrders.Count();

        if (request.pageNumber.HasValue && request.pagesize.HasValue)
        {
            int skip = (request.pageNumber.Value - 1) * request.pagesize.Value;
            allOrders = allOrders.Skip(skip).Take(request.pagesize.Value).ToList();
        }

        var response = allOrders.Select(order => new OrderModelDto.OrderReadResponse(
            order.Customer?.Name ?? "Desconocido",
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.TotalAmount,
            order.OrderItems.Select(oi => new OrderItemsModelDto.OrderItemResponse(
                oi.ProductID,
                oi.Product?.Name ?? string.Empty,
                oi.Quantity,
                oi.UnitPrice
            )).ToList(),
            order.OrderStatus
        )).ToList();

        return new OrderModelDto.OrderReadListResponse(total, response);
    }


}