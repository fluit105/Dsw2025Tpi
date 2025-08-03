using Dsw2025Tpi.Domain.Entities;
using System.Collections.Generic;

namespace Dsw2025Tpi.Domain.Domain;

public class Customer : EntityBase
{
      // Necesario para que EF Core pueda instanciar al hidratar desde la base.
#pragma warning disable CS8618
      public Customer() { }
#pragma warning restore CS8618

      // Forma clara de crear un cliente con sus datos principales.
      public Customer(string email, string name, string phoneNumber)
      {
            Email = email;
            Name = name;
            PhoneNumber = phoneNumber;
      }

      public string Email { get; set; }
      public string Name { get; set; }
      public string PhoneNumber { get; set; }

      // Un cliente puede tener muchas órdenes. 
      public List<Order>? Orders { get; set; }
}
