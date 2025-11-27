using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Domain;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services;

// Servicio encargado de la gestión de clientes.
// Implementa la interfaz ICustomerManagmentsService.
public class CustomerManagmentsService : ICustomerManagmentsService
{
    private readonly IRepository _repository;
    private readonly ILogger<CustomerManagmentsService> _logger;

    // Recibe el repositorio por inyección de dependencias.
    // Esto permite trabajar de forma desacoplada de la infraestructura de datos.
    // Internamente desde Program.cs se inyecta un EFRepository que implementa IRepository.
    public CustomerManagmentsService(IRepository repository, ILogger<CustomerManagmentsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // Crea un nuevo cliente en el sistema.
    // Devuelve la entidad Customer creada.
    public async Task<Customer> CreateCustomerAsync(string email, string name, string phoneNumber)
    {
        _logger.LogInformation("Creando cliente: {Email}, {Name}", email, name);

        // Crea la entidad de dominio con los datos proporcionados.
        var customer = new Customer(email, name, phoneNumber);
        var savedCustomer = await _repository.Add(customer);

        // Devuelve la entidad creada.
        return savedCustomer;
    }
}
