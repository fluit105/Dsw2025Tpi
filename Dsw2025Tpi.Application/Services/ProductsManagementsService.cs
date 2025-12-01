using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services;

// Servicio de aplicación para la gestión de productos.
// Implementa todas las operaciones definidas en IProductsManagementsService.
public class ProductsManagementsService : IProductsManagementsService
{
    private readonly IRepository _repository;
    private readonly ILogger<ProductsManagementsService> _logger;

    // Recibe el repositorio por inyección de dependencias.
    // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
    public ProductsManagementsService(IRepository repository, ILogger<ProductsManagementsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // Obtiene un producto por su ID.
    public async Task<Product?> GetProductById(Guid id) =>
        await _repository.GetById<Product>(id);

    // Obtiene todos los productos activos.
    public async Task<IEnumerable<Product>?> GetAllProducts() =>
        await _repository.GetFiltered<Product>(p => p.IsActive);

    // Actualiza un producto ya existente.
    public async Task<Product> UpdateProduct(Product product) =>
        await _repository.Update(product);

    // Elimina un producto.
    public async Task<Product> DeleteProduct(Product product) =>
        await _repository.Delete(product);

    public async Task<IEnumerable<Product>> SearchProducts(string term) => 
        await _repository.GetFiltered<Product>(
            p => p.Name.Contains(term) || p.Sku.Contains(term)
        );

    public async Task<int> GetProductCountAsync()
    {
        var products = await _repository.GetFiltered<Product>(p => p.IsActive); // o GetAll<Product>() para todos
        return products?.Count() ?? 0;
    }


    // Obtiene un producto por su SKU.
    public async Task<Product?> GetProductBySku(string sku) =>
        await _repository.First<Product>(p => p.Sku == sku);

    // Agrega un nuevo producto al sistema.
    public async Task<ProductModelDto.ProductResponseWithDescription> AddProduct(ProductModelDto.ProductRequestWithDescription request)
    {
        _logger.LogInformation("Creando producto SKU={Sku}, Name={Name}", request.Sku, request.Name);

        if (!IsValid(request))
        {
            _logger.LogWarning("Datos inválidos para crear producto SKU={Sku}", request.Sku);
            throw new ArgumentException("Valores para el producto no válidos");
        }

        // Verifica si ya existe un producto con el mismo SKU.
        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null)
        {
            _logger.LogWarning("SKU duplicado detectado: {Sku}", request.Sku);
            throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        }

        // Crea la entidad de dominio Product.
        var product = new Product(
            request.Sku,
            request.InternalCode,
            request.Description,
            request.Name,
            request.Price,
            (int)request.Stock
        );

        // Guarda el nuevo producto en la base de datos.
        await _repository.Add(product);

        _logger.LogInformation("Producto creado correctamente con Id={Id} SKU={Sku}", product.Id, product.Sku);

        // Devuelve un DTO con los datos del producto creado.
        return new ProductModelDto.ProductResponseWithDescription(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive
        );
    }

    // Modifica un producto existente con los nuevos datos recibidos.
    public async Task<Product> ModifyProduct(Product product, ProductModelDto.ProductRequestWithDescription request)
    {
        if (!IsValid(request))
        {
            _logger.LogWarning("Datos inválidos para modificar producto Id={Id}", product.Id);
            throw new ArgumentException("Valores para el producto no válidos");
        }

        // Verifica que no haya otro producto con el mismo SKU.
        var exist = await _repository.First<Product>(p => p.Sku == request.Sku && p.Id != product.Id);
        if (exist != null)
        {
            _logger.LogWarning("Conflicto de SKU en modificación: SKU={Sku}, Id existente={ExistingId}", request.Sku, exist.Id);
            throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        }

        // Actualiza los campos de la entidad.
        product.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.Price;
        product.StockQuantity = (int)request.Stock;

        // Guarda cambios.
        var updated = await UpdateProduct(product);
        _logger.LogInformation("Producto actualizado Id={Id}", updated.Id);
        return updated;
    }

    // Cambia el estado activo/inactivo de un producto.
    public async Task PatchProductIsActive(Product product)
    {
        product.ToggleIsActive();
        await UpdateProduct(product);
        _logger.LogInformation("Producto Id={Id} nuevo estado IsActive={IsActive}", product.Id, product.IsActive);
    }

    // Valida los datos básicos del producto.
    private bool IsValid(ProductModelDto.ProductRequestWithDescription request)
    {
        return !string.IsNullOrWhiteSpace(request.Sku) &&
               !string.IsNullOrWhiteSpace(request.Name) &&
               request.Price > 0 &&
               request.Stock >= 1;
    }
    public async Task<ProductModelDto.ResponsePagination?> GetProducts(ProductModelDto.FilterProduct request)
    {
        bool? isActive = request.Status?.ToLower() switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => null
        };

        _logger.LogInformation("Consulta de productos");

        var filtered = await _repository.GetFiltered<Product>(p =>
            (isActive == null || p.IsActive == isActive) &&
            (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search))
        );

        if (filtered == null || !filtered.Any())
            throw new NotExistException("No hay productos que coincidan con los filtros");

        var projected = filtered
            .Select(p => new ProductModelDto.ResponseProduct(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            ))
            .OrderBy(p => p.Sku)
            .ToList();

        int page = request.PageNumber ?? 1;
        int size = request.PageSize ?? 10;
        int skip = (page - 1) * size;

        var paged = projected.Skip(skip).Take(size).ToList();

        return new ProductModelDto.ResponsePagination(paged, projected.Count);
    }
    public async Task<ProductModelDto.ResponsePagination?> GetActiveProductsPaginated(ProductModelDto.FilterProduct request)
    {
        // Creamos una nueva instancia del DTO de filtro para evitar modificar la original
        // y forzamos el Status a "enabled" (activo), rehusando el campo de búsqueda y paginación.
        var activeFilter = request with
        {
            Status = "enabled" // Forzamos el estado a activo, que tu GetProducts ya sabe interpretar.
        };

        // Llamamos al método de paginación existente con el filtro modificado.
        return await GetProducts(activeFilter);
    }

}