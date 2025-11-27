using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Domain;

namespace Dsw2025Tpi.Application.Interfaces
{
      // Interfaz para el servicio de gestión de órdenes.
      // Define las operaciones disponibles para crear, consultar, actualizar y filtrar órdenes.
      public interface IOrderManagementsService
      {
            // Crea una nueva orden en el sistema a partir de un DTO de solicitud.
            // Devuelve un DTO con la información detallada de la orden creada.
            Task<OrderModelDto.OrderResponse> AddOrder(OrderModelDto.OrderRequest request);

            // Obtiene una orden por su identificador único (GUID).
            // Devuelve la entidad de dominio Order o null si no existe.
            Task<Order?> GetOrderById(Guid id);

            // Actualiza el estado de una orden existente.
            // Recibe el ID de la orden y un DTO con el nuevo estado.
            // Devuelve la orden actualizada como DTO de respuesta.
            Task<OrderModelDto.OrderResponse> UpdateOrderStatus(Guid id, OrderModelDto.OrderStatusUpdateRequest request);

            // Obtiene una lista de órdenes aplicando filtros opcionales (estado, cliente, paginación).
            // Devuelve una lista de DTOs de respuesta listos para la API.
            Task<List<OrderModelDto.OrderResponse>> GetAllOrdersFilters(OrderModelDto.OrderFilterRequest request);

        Task<OrderModelDto.OrderListResponse> GetAllOrdersFilter(OrderModelDto.OrderFilterRequest request);


        Task<OrderModelDto.OrderReadListResponse> GetAllOrdersWithCustomerName(OrderModelDto.OrderFilterRequestName request);

        Task<int> GetOrderCountAsync();
    }
}
