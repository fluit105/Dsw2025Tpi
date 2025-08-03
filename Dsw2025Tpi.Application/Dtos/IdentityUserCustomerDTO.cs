using Dsw2025Tpi.Domain.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Dsw2025Tpi.Application.Dtos;

// Usuario personalizado para Identity
// Hereda de IdentityUser y agrega un vínculo con la entidad Customer del dominio.
public class IdentityUserCustomerDto : IdentityUser
{
      // Identificador del cliente en el dominio
      // Permite asociar este usuario de autenticación con un registro de Customer.
      public Guid CustomerId { get; set; }
}
