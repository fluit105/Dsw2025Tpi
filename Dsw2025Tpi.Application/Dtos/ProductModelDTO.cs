using System;

namespace Dsw2025Tpi.Application.Dtos;

// DTO maestro para productos en la API.
// Agrupa variantes para solicitudes y respuestas con distintos niveles de detalle.
public record ProductModelDto
{
      // Petición de producto para usar dentro de un pedido/orden.
      // Incluye cantidad solicitada y algunos datos del producto.
      public record ProductRequest(
          Guid id,
          int quantity,
          string name,
          decimal currentunitPrice
      );

      // Petición completa para crear o actualizar un producto en el catálogo.
      public record ProductRequestWithDescription(
          string Sku,
          string InternalCode,
          string Name,
          string Description,
          decimal Price,
          decimal Stock
      );

      // Respuesta mínima: solo el identificador del producto.
      public record ProductResponse(Guid Id);

      // Respuesta detallada con todos los datos de producto.
      public record ProductResponseWithDescription(
          Guid Id,
          string Sku,
          string InternalCode,
          string Name,
          string Description,
          decimal Price,
          decimal Stock,
          bool IsActive
      );

    public record FilterProduct(string? Status, string? Search, int? PageNumber, int? PageSize);

    public record ResponseProduct(
        Guid Id,
        string? Sku,
        string? InternalCode,
        string? Name,
        string? Description,
        decimal? CurrentUnitPrice,
        int? StockQuantity,
        bool IsActive
    );

    public record ResponsePagination(List<ResponseProduct> ProductItems, int Total);

}
