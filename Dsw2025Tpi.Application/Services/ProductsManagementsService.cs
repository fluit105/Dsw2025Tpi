using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

// Servicio de aplicación para la gestión de productos.
// Implementa todas las operaciones definidas en IProductsManagementsService.
public class ProductsManagementsService : IProductsManagementsService
{
      private readonly IRepository _repository;

      // Recibe el repositorio por inyección de dependencias.
      // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
      public ProductsManagementsService(IRepository repository)
      {
            _repository = repository;
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

      // Obtiene un producto por su SKU.
      public async Task<Product?> GetProductBySku(string sku) =>
          await _repository.First<Product>(p => p.Sku == sku);

      // Agrega un nuevo producto al sistema.
      public async Task<ProductModelDto.ProductResponseWithDescription> AddProduct(ProductModelDto.ProductRequestWithDescription request)
      {
            if (!IsValid(request))
                  throw new ArgumentException("Valores para el producto no válidos");

            // Verifica si ya existe un producto con el mismo SKU.
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null)
                  throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

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
                  throw new ArgumentException("Valores para el producto no válidos");

            // Verifica que no haya otro producto con el mismo SKU.
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku && p.Id != product.Id);
            if (exist != null)
                  throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

            // Actualiza los campos de la entidad.
            product.Sku = request.Sku;
            product.InternalCode = request.InternalCode;
            product.Name = request.Name;
            product.Description = request.Description;
            product.CurrentUnitPrice = request.Price;
            product.StockQuantity = (int)request.Stock;

            // Guarda cambios.
            return await UpdateProduct(product);
      }

      // Cambia el estado activo/inactivo de un producto.
      public async Task PatchProductIsActive(Product product)
      {
            product.ToggleIsActive();
            await UpdateProduct(product);
      }

      // Valida los datos básicos del producto.
      private bool IsValid(ProductModelDto.ProductRequestWithDescription request)
      {
            return string.IsNullOrWhiteSpace(request.Sku) &&
                   string.IsNullOrWhiteSpace(request.Name) &&
                   request.Price > 0 &&
                   request.Stock >= 1;
      }
}
