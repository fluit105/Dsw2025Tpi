using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Domain;

namespace Dsw2025Tpi.Application.Interfaces
{
      // Interfaz para el servicio de gestión de productos.
      // Define las operaciones disponibles para manejar productos en la aplicación.
      public interface IProductsManagementsService
      {
            // Agrega un nuevo producto al sistema a partir de un DTO con la descripción completa.
            // Devuelve un DTO con la información detallada del producto creado.
            Task<ProductModelDto.ProductResponseWithDescription> AddProduct(ProductModelDto.ProductRequestWithDescription request);

            // Elimina un producto existente.
            // Devuelve la entidad de dominio eliminada.
            Task<Product> DeleteProduct(Product product);

            // Obtiene todos los productos del sistema.
            // Devuelve una colección de entidades de dominio Product.
            Task<IEnumerable<Product>?> GetAllProducts();

            // Busca un producto por su identificador único.
            // Devuelve la entidad encontrada o null si no existe.
            Task<Product?> GetProductById(Guid id);

            // Busca un producto por su SKU (código único).
            // Devuelve la entidad encontrada o null si no existe.
            Task<Product?> GetProductBySku(string sku);

            // Modifica un producto existente usando un DTO con la nueva información.
            // Devuelve la entidad actualizada.
            Task<Product> ModifyProduct(Product product, ProductModelDto.ProductRequestWithDescription request);

            // Cambia el estado de activación de un producto (activo/inactivo).
            Task PatchProductIsActive(Product product);

            // Actualiza un producto ya modificado previamente (por ejemplo, tras manipularlo en memoria).
            // Devuelve la entidad actualizada.
            Task<Product> UpdateProduct(Product product);
      }
}
