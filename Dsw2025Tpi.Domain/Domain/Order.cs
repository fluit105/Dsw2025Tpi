using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Domain.Domain;

public class Order : EntityBase
{
      // Asociada permanentemente al cliente que la genera.
      public Guid CustomerId { get; init; }

      // Marca temporal de creación; no se muta después.
      public DateTime Date { get; init; } = DateTime.Now;

      public string ShippingAddress { get; set; }
      public string BillingAddress { get; set; }

      public string? Notes { get; set; }

      // Derivado de OrderItems en el flujo de negocio.
      public decimal TotalAmount { get; set; }

      // Detalle de la orden: los renglones con producto, cantidad y precio histórico.
      public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

      // Estado del ciclo de vida.
      public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING;

      // Navegación al cliente propietario.
      public required Customer Customer { get; set; }

      // Inicializa la orden con sus referencias fundamentales.
      public Order(Guid customerId, string shippingAddress, string billingAddress)
      {
            CustomerId = customerId;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
      }
}
