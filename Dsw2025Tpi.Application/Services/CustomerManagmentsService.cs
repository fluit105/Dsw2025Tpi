using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

// Servicio encargado de la gestión de clientes.
// Implementa la interfaz ICustomerManagmentsService.
public class CustomerManagmentsService : ICustomerManagmentsService
{
      private readonly IRepository _repository;

      // Recibe el repositorio por inyección de dependencias.
      // Esto permite trabajar de forma desacoplada de la infraestructura de datos.
      // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
      public CustomerManagmentsService(IRepository repository)
      {
            _repository = repository;
      }

      // Crea un nuevo cliente en el sistema.
      // Devuelve la entidad Customer creada.
      public async Task<Customer> CreateCustomerAsync(string email, string name, string phoneNumber)
      {
            // Crea la entidad de dominio con los datos proporcionados.
            var customer = new Customer(email, name, phoneNumber);

            // Persiste el cliente en la base de datos.
            await _repository.Add(customer);

            // Devuelve la entidad creada.
            return customer;
      }
}
