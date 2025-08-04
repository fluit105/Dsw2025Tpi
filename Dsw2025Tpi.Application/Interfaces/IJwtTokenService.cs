namespace Dsw2025Tpi.Application.Interfaces
{
      // Contrato para un servicio que genera tokens JWT.
      // Usado en el flujo de autenticación para emitir el token
      // que el cliente usará en cada petición protegida.
      public interface IJwtTokenService
      {
            // Genera un JWT para un usuario dado.
            string GenerateToken(string userId, string username);
      }
}
