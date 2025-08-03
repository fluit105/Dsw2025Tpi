using Dsw2025Tpi.Domain.Domain;
using Microsoft.EntityFrameworkCore;

public class DomainContext : DbContext
{
      public DomainContext(DbContextOptions<DomainContext> options) : base(options) { }

      // Conjuntos de entidades (tablas) del dominio
      public DbSet<Customer> Customers { get; set; }
      public DbSet<Product> Products { get; set; }
      public DbSet<Order> Orders { get; set; }
      public DbSet<OrderItem> OrderItems { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            // Configuración de Product
            modelBuilder.Entity<Product>(p =>
            {
                  p.ToTable("Products");  // Nombre de la tabla
                  p.Property(p => p.Sku)  // Propiedad SKU
                      .IsRequired()       // Obligatorio
                      .HasMaxLength(20)   // Máximo 20 caracteres
                      .IsUnicode();       // Permitir caracteres Unicode
                  p.Property(p => p.Id)   // Clave primaria
                      .IsRequired();
                  p.Property(p => p.Name) // Nombre del producto
                      .IsRequired()
                      .HasMaxLength(100);

                  // Índice único para SKU
                  p.HasIndex(x => x.Sku).IsUnique();
            });

            // Configuración de Order
            modelBuilder.Entity<Order>(o =>
            {
                  o.ToTable("Orders");                  // Nombre de la tabla
                  o.Property(o => o.CustomerId)         // FK al cliente
                      .IsRequired();
                  o.HasKey(o => o.Id);                  // Clave primaria

                  // Relación: Order -> Customer (muchas órdenes para un cliente)
                  o.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade); // Si se borra el cliente, borrar órdenes

                  o.Property(o => o.ShippingAddress)     // Dirección de envío
                      .IsRequired()
                      .HasMaxLength(200);
                  o.Property(o => o.BillingAddress)      // Dirección de facturación
                      .IsRequired()
                      .HasMaxLength(200);
                  o.Property(o => o.TotalAmount)         // Total de la orden
                      .IsRequired();
            });

            // Configuración de OrderItem
            modelBuilder.Entity<OrderItem>(oi =>
            {
                  oi.ToTable("OrderItems");              // Nombre de la tabla
                  oi.Property(oi => oi.ProductID)        // FK al producto
                      .IsRequired();
                  oi.HasKey(oi => oi.Id);                // Clave primaria

                  // Relación: OrderItem -> Product
                  oi.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductID)
                      .OnDelete(DeleteBehavior.Cascade); // Si se borra el producto, borrar ítems

                  // Relación: OrderItem -> Order
                  oi.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderID)
                      .OnDelete(DeleteBehavior.Cascade); // Si se borra la orden, borrar ítems

                  oi.Property(oi => oi.Quantity)         // Cantidad
                      .IsRequired();
                  oi.Property(oi => oi.UnitPrice)        // Precio unitario
                      .IsRequired();
            });

            // Configuración de Customer
            modelBuilder.Entity<Customer>(c =>
            {
                  c.ToTable("Customers");        // Nombre de la tabla
                  c.Property(c => c.Name)        // Nombre del cliente
                      .IsRequired()
                      .HasMaxLength(100);
                  c.Property(c => c.Id)          // Clave primaria
                      .IsRequired();
                  c.Property(c => c.Email)       // Email del cliente
                      .IsRequired()
                      .HasMaxLength(100)
                      .IsUnicode(false);         // ASCII
                  c.Property(c => c.PhoneNumber) // Teléfono
                      .HasMaxLength(15)
                      .IsUnicode(false);

                  // Índice único para Email
                  c.HasIndex(x => x.Email).IsUnique();
            });
      }
}