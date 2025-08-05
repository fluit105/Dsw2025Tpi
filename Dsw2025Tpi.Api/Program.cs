using Dsw2025Tpi.Application.Dtos;                   // Modelos DTO de la capa Application
using Dsw2025Tpi.Application.Interfaces;             // Interfaces de servicios de la capa Application
using Dsw2025Tpi.Application.Services;               // Implementaciones de servicios de la capa Application
using Dsw2025Tpi.Data.Context;                       // Contexto de autenticación (Identity)
using Dsw2025Tpi.Data.Repositories;                  // Implementación de repositorios
using Dsw2025Tpi.Domain.Interfaces;                  // Interfaces del dominio
using Microsoft.AspNetCore.Authentication.JwtBearer; // Autenticación JWT
using Microsoft.AspNetCore.Identity;                 // ASP.NET Core Identity
using Microsoft.EntityFrameworkCore;                 // Entity Framework Core
using Microsoft.IdentityModel.Tokens;                // Configuración de validación de tokens JWT
using Microsoft.OpenApi.Models;                      // Swagger y OpenAPI
using System.Text;                                   // Codificación para claves JWT
using System.Threading.RateLimiting;                 // Limitación de tasa para proteger la API
using Microsoft.AspNetCore.RateLimiting;             // Limitación de tasa para ASP.NET Core
using AspNetCoreRateLimit;                           // Biblioteca para limitación de tasa por IP

namespace Dsw2025Tpi.Api;

public static class Program
{
      public static void Main(string[] args) // Lo hago async para poder ejecutar seeding con await
      {
            var builder = WebApplication.CreateBuilder(args);
            // Crea el objeto principal de configuración de la aplicación.
            // Carga configuración desde appsettings, variables de entorno y argumentos.
            // Prepara el servidor web (Kestrel) y sistema de logging.
            // Expone "builder.Services" para registrar servicios en el contenedor de DI.
            // Expone "builder.Configuration" y "builder.Environment" para acceder a config y entorno.

            builder.Services.AddRateLimiter(options =>
            {
                  // Código de rechazo cuando supere el límite
                  options.RejectionStatusCode = 429;

                  // Política global por IP: 10 peticiones / 10 segundos por dirección IP
                  options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                        // Particionar por IP
                        PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        {
                              // Clave = la IP remota
                              var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                              // Cada partición (IP) tiene su propia ventana fija
                              return RateLimitPartition.GetFixedWindowLimiter(
                                    partitionKey: ip,
                                    factory: partition => new FixedWindowRateLimiterOptions
                                    {
                                          PermitLimit = 10,
                                          Window = TimeSpan.FromSeconds(10),
                                          QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                          QueueLimit = 0
                                    }
                              );
                        })
                  );
            });


            // Definir un nombre para la política
            const string CorsPolicyName = "CorsPolicy";

            builder.Services.AddCors(options =>
            {
                  options.AddPolicy(CorsPolicyName, policy =>
                  {
                        policy
                          .WithOrigins(
                            "https://mi-frontend.com",     // Dominio de producción
                            "https://localhost:4200"       // Dominio de desarrollo (Angular, React, etc.)
                          )
                          .AllowAnyHeader()                // Permitir todos los encabezados que tu cliente necesite
                          .AllowAnyMethod()                // Permitir GET, POST, PUT, PATCH, DELETE…
                          .AllowCredentials();             // Solo si usas cookies o autenticación basada en credenciales
                  });
            });


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

            // Sólo en producción habilitamos CORS
            if (app.Environment.IsProduction())
            {
                  app.UseCors(CorsPolicyName);
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/healthcheck");

#pragma warning disable S6966
            app.Run();
#pragma warning restore S6966
      }
}
