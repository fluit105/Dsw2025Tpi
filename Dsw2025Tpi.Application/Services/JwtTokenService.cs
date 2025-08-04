using Dsw2025Tpi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dsw2025Tpi.Application.Services;

// Servicio encargado de generar tokens JWT para la autenticación.
// Implementa la interfaz IJwtTokenService.
public class JwtTokenService : IJwtTokenService
{
      private readonly IConfiguration _config;

      // Recibe IConfiguration por inyección de dependencias
      // para poder leer la configuración desde appsettings.json.
      public JwtTokenService(IConfiguration config)
      {
            _config = config ?? throw new ArgumentNullException(nameof(config));
      }

      // Genera un token JWT para un usuario y rol determinados.
      public string GenerateToken(string userName, string role)
      {
            // Obtiene la configuración de la sección "Jwt".
            var jwtConfig = _config.GetSection("Jwt");

            // Lee la clave secreta y valida que exista.
            var keyText = jwtConfig["Key"] ?? throw new ArgumentException("JWT Key is not configured");

            // Convierte la clave a un objeto de seguridad simétrica.
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyText));

            // Configura las credenciales de firma usando el algoritmo HMAC-SHA256.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define las claims (información que viajará dentro del token).
            var claim = new[]
            {
                  // Identificador del usuario (subject).
                  new Claim(JwtRegisteredClaimNames.Sub, userName),

                  // Identificador único del token (para prevenir reutilización).
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                  // Rol del usuario para control de acceso.
                  new Claim(ClaimTypes.Role, role)
            };

            // Crea el token con la configuración establecida.
            var token = new JwtSecurityToken(
                  issuer: jwtConfig["Issuer"],           // Emisor del token.
                  audience: jwtConfig["Audience"],       // Audiencia prevista.
                  claims: claim,
                  // Tiempo de expiración (por defecto 60 minutos).// Claims incluidas.
                  expires: DateTime.Now.AddMinutes(
                        double.Parse(jwtConfig["ExpireInMinutes"] ?? "60") 
                  ),
                  signingCredentials: creds              // Credenciales de firma.
            );

            // Serializa el token a string para enviarlo al cliente.
            return new JwtSecurityTokenHandler().WriteToken(token);
      }
}
