using Dsw2025Tpi.Application.Dtos;                   // Modelos DTO de la capa Application
using Dsw2025Tpi.Application.Interfaces;             // Interfaces de servicios de la capa Application
using Dsw2025Tpi.Application.Services;               // Implementaciones de servicios de la capa Application
using Dsw2025Tpi.Data.Context;                       // Contexto de autenticación (Identity)
using Dsw2025Tpi.Data.Repositories;                  // Implementación de repositorios
using Dsw2025Tpi.Data.Seeding;
using Dsw2025Tpi.Domain.Interfaces;                  // Interfaces del dominio
using Microsoft.AspNetCore.Authentication.JwtBearer; // Autenticación JWT
using Microsoft.AspNetCore.Identity;                 // ASP.NET Core Identity
using Microsoft.EntityFrameworkCore;                 // Entity Framework Core
using Microsoft.IdentityModel.Tokens;                // Configuración de validación de tokens JWT
using Microsoft.OpenApi.Models;                      // Swagger y OpenAPI
using System.Text;                                   // Codificación para claves JWT
using Volo.Abp.Data;                                 // ABP Framework para uso de seeds

namespace Dsw2025Tpi.Api;

public static class Program
{
      public static async Task Main(string[] args) // Lo hago async para poder ejecutar seeding con await
      {
            var builder = WebApplication.CreateBuilder(args);
            // Crea el objeto principal de configuración de la aplicación.
            // Carga configuración desde appsettings, variables de entorno y argumentos.
            // Prepara el servidor web (Kestrel) y sistema de logging.
            // Expone "builder.Services" para registrar servicios en el contenedor de DI.
            // Expone "builder.Configuration" y "builder.Environment" para acceder a config y entorno.

            builder.Services.AddControllers();            // Registra los controladores de la API
            builder.Services.AddEndpointsApiExplorer();   // Permite que Swagger descubra los endpoints

            // Configura Swagger para documentar la API y habilitar autenticación JWT
            builder.Services.AddSwaggerGen(o =>
            {
                  o.SwaggerDoc("v1", new OpenApiInfo
                  {
                        Title = "Desarrollo de Software",
                        Version = "v1",
                  });
                  o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                  {
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Ingresar el token JWT con el formato: Bearer {token}",
                        Type = SecuritySchemeType.ApiKey
                  });
                  o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            builder.Services.AddHealthChecks();

            builder.Services.AddIdentity<IdentityUserCustomerDto, IdentityRole>(options =>
            {
                  options.Password = new PasswordOptions
                  {
                        RequiredLength = 8,
                  };
            })
            .AddEntityFrameworkStores<AuthenticateContext>()
            .AddDefaultTokenProviders();

            var jwtConfig = builder.Configuration.GetSection("Jwt");
            var keyText = jwtConfig["Key"] ?? throw new ArgumentException("JWT Key");
            var key = Encoding.UTF8.GetBytes(keyText);

            builder.Services.AddAuthentication(options =>
            {
                  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                  };
            });

            builder.Services.AddDbContext<DomainContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConectionString")));

            builder.Services.AddTransient<IProductsManagementsService, ProductsManagementsService>();
            builder.Services.AddTransient<IOrderManagementsService, OrderManagementsService>();
            builder.Services.AddTransient<ICustomerManagmentsService, CustomerManagmentsService>();
            builder.Services.AddScoped<IRepository, EfRepository>();
            builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

            builder.Services.AddDbContext<AuthenticateContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConectionString")));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                  app.UseSwagger();
                  app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/healthcheck");

            // Ejecutamos el seeding de datos solo en modo Development
            if (app.Environment.IsDevelopment())
            {
                  using (var scope = app.Services.CreateScope())
                  {
                        var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                        await dataSeeder.SeedAsync();
                  }
            }

#pragma warning disable S6966
            app.Run();
#pragma warning restore S6966
      }
}
