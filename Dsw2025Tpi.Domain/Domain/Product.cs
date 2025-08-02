using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Domain.Domain;

public class Product : EntityBase
{
      public string Sku { get; set; }
      public string InternalCode { get; set; }
      public string Description { get; set; }
      public string Name { get; set; }

      // Precio actual del producto, se usa al momento de construir un OrderItem para fijar el precio histórico.
      public decimal CurrentUnitPrice { get; set; }

      public int StockQuantity { get; set; }

      public Boolean IsActive { get; private set; } = true;

      // Navegación inversa: todos los order items donde este producto fue usado.
      public List<OrderItem>? OrderItems { get; set; }

      // Constructor requerido por EF Core para materialización.
      protected Product() { }

      // Creación explícita con los datos base necesarios para tener un producto consistente.
      public Product(string sku, string internalCode, string description, string name, decimal currentunitPrice, int stockQuantity)
      {
            Sku = sku;
            this.InternalCode = internalCode;
            Description = description;
            Name = name;
            CurrentUnitPrice = currentunitPrice;
            StockQuantity = stockQuantity;
      }

      // Cambio de estado activo/inactivo.
      public void ToggleIsActive() => IsActive = !IsActive;
}
