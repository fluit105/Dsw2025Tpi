using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Domain.Domain;

public class OrderItem : EntityBase
{
      // Referencia al producto comprado; no se modifica luego de creado.
      public Guid ProductID { get; init; }

      // Referencia a la orden que contiene este renglón.
      public Guid OrderID { get; init; }

      public int Quantity { get; set; }

      // Precio unitario en el momento de la compra (precio histórico, no se recalcula desde el producto).
      public decimal UnitPrice { get; set; }

      public decimal Subtotal => Quantity * UnitPrice;

      // Navegación para cuando se cargan relaciones completas.
      public required Customer Order { get; set; }
      public required Customer Product { get; set; }

      // Constructor parameterless requerido para que EF Core pueda instanciar durante materialización.
      public OrderItem() { }

      // Constructor de conveniencia para armar el ítem con sus referencias y valores clave.
      public OrderItem(Guid productId, Guid orderId, int quantity, decimal currentunitPrice)
      {
            ProductID = productId;
            OrderID = orderId;
            Quantity = quantity;
            UnitPrice = currentunitPrice;
      }
}
