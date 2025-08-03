using System;

namespace Dsw2025Tpi.Application.Dtos;

// DTO agrupador para representar distintas vistas de los ítems de una orden.
public record OrderItemsModelDto
{
      // Representación completa de un ítem de orden.
      // Incluye IDs de producto y orden, cantidad y precio unitario.
      public record OrderItem(Guid ProductId, Guid OrderId, int Quantity, decimal UnitPrice);

      // Datos que envía el cliente al crear una orden.
      // No incluye OrderId ni UnitPrice porque los asigna el backend.
      public record OrderItemRequest(Guid ProductId, int Quantity);

      // Datos que devuelve la API al cliente sobre un ítem de orden.
      // Incluye nombre del producto para mostrarlo en UI.
      public record OrderItemResponse(Guid ProductId, string Name, int Quantity, decimal UnitPrice);
}
