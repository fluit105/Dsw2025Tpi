using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Data.Context;
using Dsw2025Tpi.Domain.Domain;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Dsw2025Tpi.Data.Seeding;

public class AppDataSeeder : IDataSeedContributor, ITransientDependency
{
      private readonly RoleManager<IdentityRole> _roleManager;
      private readonly UserManager<IdentityUserCustomerDto> _userManager;
      private readonly DomainContext _domainContext;

      public AppDataSeeder(
          RoleManager<IdentityRole> roleManager,
          UserManager<IdentityUserCustomerDto> userManager,
          DomainContext domainContext)
      {
            _roleManager = roleManager;
            _userManager = userManager;
            _domainContext = domainContext;
      }

      public async Task SeedAsync(DataSeedContext context)
      {
            // 1. Crear rol Admin si no existe
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                  await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Crear usuario admin si no existe
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                  var customer = new Customer("admin@example.com", "Administrador", "000000000");
                  _domainContext.Customers.Add(customer);
                  await _domainContext.SaveChangesAsync();

                  adminUser = new IdentityUserCustomerDto
                  {
                        UserName = "admin",
                        Email = "admin@example.com",
                        CustomerId = customer.Id
                  };
                  var result = await _userManager.CreateAsync(adminUser, "Admin123*");
                  if (result.Succeeded)
                  {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                  }
            }

            // 3. Agregar productos de ejemplo si no existen
            if (!_domainContext.Products.Any())
            {
                  _domainContext.Products.Add(new Product(
                      "SKU001",
                      "INT001",
                      "Producto de ejemplo 1",
                      "Producto 1",
                      100.50m,
                      50
                  ));
                  _domainContext.Products.Add(new Product(
                      "SKU002",
                      "INT002",
                      "Producto de ejemplo 2",
                      "Producto 2",
                      200.00m,
                      30
                  ));
                  await _domainContext.SaveChangesAsync();
            }
      }
}
