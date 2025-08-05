using Dsw2025Tpi.Application.Dtos;                   // Modelos DTO de la capa Application
using Dsw2025Tpi.Application.Interfaces;             // Interfaces de servicios de la capa Application
using Dsw2025Tpi.Application.Services;               // Implementaciones de servicios de la capa Application
using Dsw2025Tpi.Data;                               // Contexto de datos del dominio
using Dsw2025Tpi.Data.Context;                       // Contexto de autenticaci�n (Identity)
using Dsw2025Tpi.Data.Repositories;                  // Implementaci�n de repositorios
using Dsw2025Tpi.Domain.Interfaces;                  // Interfaces del dominio
using Microsoft.AspNetCore.Authentication.JwtBearer; // Autenticaci�n JWT
using Microsoft.AspNetCore.Identity;                 // ASP.NET Core Identity
using Microsoft.EntityFrameworkCore;                 // Entity Framework Core
using Microsoft.IdentityModel.Tokens;                // Configuraci�n de validaci�n de tokens JWT
using Microsoft.OpenApi.Models;                      // Swagger y OpenAPI
using System.Text;                                   // Codificaci�n para claves JWT
using Volo.Abp.Data;                                 // ABP Framework para uso de seeds

namespace Dsw2025Tpi.Api;

public static class Program
{
      public static void Main(string[] args)
      {
            var builder = WebApplication.CreateBuilder(args);
            // Crea el objeto principal de configuraci�n de la aplicaci�n.
            // Carga configuraci�n desde appsettings, variables de entorno y argumentos.
            // Prepara el servidor web (Kestrel) y sistema de logging.
            // Expone "builder.Services" para registrar servicios en el contenedor de DI.
            // Expone "builder.Configuration" y "builder.Environment" para acceder a config y entorno.


            builder.Services.AddControllers();            // Registra los controladores de la API
            builder.Services.AddEndpointsApiExplorer();   // Permite que Swagger descubra los endpoints

            // Configura Swagger para documentar la API y habilitar autenticaci�n JWT
            builder.Services.AddSwaggerGen(o =>
            {
                  // Define la informaci�n b�sica del documento OpenAPI
                  o.SwaggerDoc("v1", new OpenApiInfo
                  {
                        Title = "Desarrollo de Software", // T�tulo que ver� el usuario en Swagger UI
                        Version = "v1",                   // Versi�n de la API documentada
                  });

                  // Define el esquema de seguridad para JWT Bearer
                  o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                  {
                        In = ParameterLocation.Header,    // El token se env�a en el encabezado HTTP
                        Name = "Authorization",           // Nombre del encabezado que contendr� el token
                        Description = "Ingresar el token JWT con el formato: Bearer {token}", // Texto de ayuda
                        Type = SecuritySchemeType.ApiKey  // Tipo API Key para que Swagger
                                                          // muestre el campo de token
                  });

                  // Aplica el esquema a todas las operaciones protegidas
                  o.AddSecurityRequirement(new OpenApiSecurityRequirement
                  {
                        {
                              new OpenApiSecurityScheme
                              {
                                    Reference = new OpenApiReference // Referencia al esquema definido arriba
                                    {
                                          Type = ReferenceType.SecurityScheme,
                                          Id = "Bearer"
                                    }
                              },
                              Array.Empty<string>()                  // Sin scopes adicionales
                        }
                  });
            });


            builder.Services.AddHealthChecks(); // Permite exponer un endpoint para
                                                // verificar la salud de la API

            // Configura Identity
            builder.Services.AddIdentity<IdentityUserCustomerDto, IdentityRole>(options =>
            {
                  options.Password = new PasswordOptions
                  {
                        RequiredLength = 8,     // Longitud m�nima de contrase�a
                  };
            })

            // Usa AuthenticateContext para persistir usuarios y roles
            .AddEntityFrameworkStores<AuthenticateContext>()

            // Agrega proveedores de tokens por defecto (ej: para recuperaci�n de contrase�a)
            .AddDefaultTokenProviders();

            // Lee la secci�n JWT de appsettings.json
            var jwtConfig = builder.Configuration.GetSection("Jwt");

            // Lee la secci�n JWT de appsettings.json
            var keyText = jwtConfig["Key"] ?? throw new ArgumentException("JWT Key");

            // Convierte la clave a bytes
            var key = Encoding.UTF8.GetBytes(keyText);

            // Configura el esquema de autenticaci�n predeterminado
            builder.Services.AddAuthentication(options =>
            {
                  // Autenticaci�n JWT
                  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                  // Desaf�o JWT
                  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Configura autenticaci�n JWT
            .AddJwtBearer(options =>
            {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                        ValidateIssuer = true,                           // Valida el emisor del token
                        ValidateAudience = true,                         // Valida la audiencia del token
                        ValidateLifetime = true,                         // Valida la expiraci�n del token
                        ValidateIssuerSigningKey = true,                 // Valida la firma del token
                        ValidIssuer = jwtConfig["Issuer"],               // Emisor v�lido esperado
                        ValidAudience = jwtConfig["Audience"],           // Audiencia v�lida esperada
                        IssuerSigningKey = new SymmetricSecurityKey(key) // Clave usada para validar la firma
                  };
            });

            // Registra el contexto de datos del dominio
            builder.Services.AddDbContext<DomainContext>(options =>
                  // Usa SQL Server
                  options.UseSqlServer(builder.Configuration.GetConnectionString("ConectionString")));

            // Servicio de productos
            builder.Services.AddTransient<IProductsManagementsService, ProductsManagementsService>();
            // Servicio de �rdenes
            builder.Services.AddTransient<IOrderManagementsService, OrderManagementsService>();
            // Servicio de clientes
            builder.Services.AddTransient<ICustomerManagmentsService, CustomerManagmentsService>();
            // Repositorio gen�rico EF
            builder.Services.AddScoped<IRepository, EfRepository>();
            // Servicio para generar JWT
            builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

            // Registra el contexto de autenticaci�n
            builder.Services.AddDbContext<AuthenticateContext>(options =>
                  // Usa SQL Server
                  options.UseSqlServer(builder.Configuration.GetConnectionString("ConectionString")));

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) // Solo en desarrollo habilita Swagger UI
            {
                  app.UseSwagger();              // Middleware para Swagger
                  app.UseSwaggerUI();            // Interfaz gr�fica de Swagger
            }

            app.UseHttpsRedirection(); // Redirige autom�ticamente a HTTPS

            app.UseAuthentication();   // Middleware de autenticaci�n (debe ir antes que autorizaci�n)

            app.UseAuthorization();    // Middleware de autorizaci�n

            app.MapControllers();      // Mapea los controladores a rutas

            app.MapHealthChecks("/healthcheck"); // Endpoint para verificar estado de la API

            app.Run();                 // Inicia la aplicaci�n
      }
}
