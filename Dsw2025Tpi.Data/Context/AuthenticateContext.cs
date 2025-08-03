using Dsw2025Tpi.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data.Context;

// DbContext especializado para ASP.NET Core Identity.
// Maneja la persistencia de usuarios, roles y toda la información
// de autenticación/autorización.
public class AuthenticateContext : IdentityDbContext<IdentityUserCustomerDTO, IdentityRole, string>
{
      // Recibe la configuración (cadena de conexión, proveedor, etc.)
      // desde Program.cs mediante inyección de dependencias.
      public AuthenticateContext(DbContextOptions<AuthenticateContext> options) : base(options) { }

      // Configuración del modelo (tablas, relaciones, etc.)
      protected override void OnModelCreating(ModelBuilder builder)
      {
            // Aplica la configuración por defecto de Identity
            base.OnModelCreating(builder);

            // Renombrar tablas por defecto de Identity para que tengan
            // nombres personalizados más claros en la base de datos.
            builder.Entity<IdentityUserCustomerDTO>   (b => { b.ToTable("Usuarios"); });
            builder.Entity<IdentityRole>              (b => { b.ToTable("Roles"); });
            builder.Entity<IdentityUserRole<string>>  (b => { b.ToTable("UsuarioRoles"); });
            builder.Entity<IdentityUserClaim<string>> (b => { b.ToTable("UsuarioClaims"); });
            builder.Entity<IdentityUserLogin<string>> (b => { b.ToTable("UsuarioLogins"); });
            builder.Entity<IdentityRoleClaim<string>> (b => { b.ToTable("RoleClaims"); });
            builder.Entity<IdentityUserToken<string>> (b => { b.ToTable("UsuarioTokens"); });
      }
}
