using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Application.Services;

// Servicio de aplicación para la gestión de órdenes.
// Implementa IOrderManagementsService.
public class OrderManagementsService : IOrderManagementsService
{
      private readonly IRepository _repository;

      // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
      public OrderManagementsService(IRepository repository)
      {
            _repository = repository;
      }

      // Caso de uso: Crear una nueva orden.
      public async Task<OrderModelDto.OrderResponse> AddOrder(OrderModelDto.OrderRequest request)
      {
            // Validar que el cliente exista
            var exist = await _repository.First<Customer>(c => c.Id == request.customerId);
            if (exist == null)
                  throw new NotExistException($"No existe el Customer con id {request.customerId}");

            // Validar direcciones de envío/facturación
            if (IsAddressValid(request))
                  throw new ArgumentException("Valores de Dirección no válidos");

            // Validar que haya productos en la orden
            if (request.Products.Count == 0)
                  throw new OrderEmptyException("No tiene ningún producto cargado al carrito");

            // Construir la orden y descontar stock
            var order = await CreateOrderAsync(request);

            // Guardar en base de datos
            await _repository.Add(order);

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

            // Procesar cada producto solicitado
            foreach (var product in request.Products)
            {
                  // Buscar producto en base de datos
                  var productespecific = await _repository.First<Product>(p => p.Id == product.id);
                  if (productespecific == null)
                        throw new NotExistException($"No existe el Producto con nombre {product.name}");

                  // Validar stock disponible
                  if (productespecific.StockQuantity < product.quantity)
                        throw new StockInsufficientException($"Stock insuficiente del Producto: {productespecific.Name}");

                  // Crear ítem de la orden con referencias requeridas
                  var orderItem = new OrderItem(
                      productespecific.Id,
                      order.Id,
                      product.quantity,
                      productespecific.CurrentUnitPrice
                  )
                  {
                        Product = productespecific, // Cumple con 'required Product'
                        Order = order               // Cumple con 'required Order'
                  };

                  // Agregar ítem a la orden
                  order.OrderItems.Add(orderItem);

                  // Sumar al total de la orden
                  order.TotalAmount += productespecific.CurrentUnitPrice * product.quantity;

                  // Descontar stock y actualizar producto
                  productespecific.StockQuantity -= product.quantity;
                  await _repository.Update(productespecific);
            }

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
                  throw new NotExistException($"No existe la orden con id: {id}");

            // Validar que el estado sea uno de los definidos en el enum OrderStatus
            if (!Enum.IsDefined(typeof(OrderStatus), request.OrderStatus))
                  throw new NotEstateExistException($"El estado de la orden {request.OrderStatus} no es válido");

            // Actualizar estado
            order.OrderStatus = request.OrderStatus;
            await _repository.Update(order);

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
      public async Task<List<OrderModelDto.OrderResponse>> GetAllOrdersFilter(OrderModelDto.OrderFilterRequest request)
      {
            // Construir predicado dinámico para filtrar resultados
            Expression<Func<Order, bool>> predicate = o =>
                (!request.OrderStatus.HasValue || o.OrderStatus == request.OrderStatus.Value) &&
                (!request.CustomerId.HasValue || o.CustomerId == request.CustomerId.Value);

            // Consultar con includes para traer los ítems de la orden
            var orders = await _repository.GetFiltered<Order>(predicate, "orderItems");

            // Si no hay resultados, devolver lista vacía
            if (orders == null || !orders.Any())
                  return new List<OrderModelDto.OrderResponse>();

            // Convertir órdenes a DTOs de respuesta
            return orders.Select(order => new OrderModelDto.OrderResponse(
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
      }
}
