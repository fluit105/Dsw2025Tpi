using Dsw2025Tpi.Application.Dtos;         // DTOs para solicitudes y respuestas de órdenes
using Dsw2025Tpi.Application.Interfaces;  // Interfaces de servicios de la capa Application
using Microsoft.AspNetCore.Authorization; // Para proteger endpoints con [Authorize]
using Microsoft.AspNetCore.Mvc;           // Funcionalidades de controladores y rutas

namespace Dsw2025Tpi.Api.Controllers;

[ApiController] // Indica que este controlador maneja solicitudes HTTP y aplica validaciones automáticas de modelo
[Authorize]     // Requiere autenticación para acceder a cualquier endpoint de este controlador
[Route("api/orders")] // Ruta base para todos los endpoints: /api/orders
public class OrderController : ControllerBase
{
    private readonly IOrderManagementsService _service; // Servicio para gestionar órdenes

    // Constructor: recibe el servicio mediante inyección de dependencias
    public OrderController(IOrderManagementsService service)
    {
        _service = service;
    }

    [HttpPost] // Indica que este endpoint responde a POST en /api/orders
    public async Task<IActionResult> addOrder([FromBody] OrderModelDto.OrderRequest request)
    {
        // Llama al servicio para crear una nueva orden
        var order = await _service.AddOrder(request);

        // Devuelve 201 Created con la URL del recurso creado
        return CreatedAtAction(nameof(GetOrderByID), new { id = order.OrderId }, order);
    }

    [HttpGet("{id}")] // Indica que este endpoint responde a GET en /api/orders/{id}
    public async Task<IActionResult> GetOrderByID(Guid id)
    {
        // Llama al servicio para obtener la orden por su ID
        var order = await _service.GetOrderById(id);

        // Si no existe, devuelve 404 Not Found
        if (order == null)
        {
            return NotFound($"No existe la orden con id: {id}");
        }

        // Si existe, devuelve 200 OK con la orden
        return Ok(order);

    }

    [HttpGet]
    public async Task<ActionResult<OrderModelDto.OrderListResponse>> GetAllOrders([FromQuery] OrderModelDto.OrderFilterRequest request)
    {
        var result = await _service.GetAllOrdersFilter(request);
        return Ok(result);
    }
    [HttpGet("count")]
    public async Task<IActionResult> GetOrderCount()
    {
        var count = await _service.GetOrderCountAsync();
        return Ok(count);
    }

    [HttpPut("{id}/status")]     // Indica que este endpoint responde a PUT en /api/orders/{id}/status
    [Authorize(Roles = "Admin")] // Solo usuarios con rol Admin pueden actualizar el estado de la orden
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderModelDto.OrderStatusUpdateRequest request)
    {
        // Llama al servicio para actualizar el estado de la orden
        var order = await _service.UpdateOrderStatus(id, request);

        // Devuelve 200 OK con la orden actualizada
        return Ok(order);

    }

    
    [HttpGet("with-customer-name")]
    public async Task<ActionResult<OrderModelDto.OrderReadListResponse>> GetOrdersWithCustomerName([FromQuery] OrderModelDto.OrderFilterRequestName request)
    {
        var result = await _service.GetAllOrdersWithCustomerName(request);
        return Ok(result);
    }
}