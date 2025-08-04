using Dsw2025Tpi.Domain.Domain;

namespace Dsw2025Tpi.Application.Interfaces
{
      // Interfaz para el servicio de gestión de clientes.
      // Define las operaciones disponibles para manejar clientes en la aplicación.
      public interface ICustomerManagmentsService
      {
            // Crea un nuevo cliente en el sistema con los datos proporcionados.
            // Devuelve la entidad de dominio Customer recién creada.
            Task<Customer> CreateCustomerAsync(string email, string name, string phoneNumber);
      }
}
