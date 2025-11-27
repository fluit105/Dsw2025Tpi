using Dsw2025Tpi.Application.Dtos;        // DTOs para entrada/salida de datos de producto
using Dsw2025Tpi.Application.Exceptions;  // Excepciones personalizadas como DuplicatedEntityException
using Dsw2025Tpi.Application.Interfaces;  // Interfaces de servicios de la capa Application
using Dsw2025Tpi.Domain.Domain;           // Entidades de dominio como Product
using Microsoft.AspNetCore.Mvc;           // Funcionalidades de controladores y atributos de rutas
using Microsoft.AspNetCore.Authorization; // Autorización para proteger los endpoints

namespace Dsw2025Tpi.Api.Controllers;

[Authorize]
[Route("api/products")] // Ruta base de este controlador
[ApiController] // Habilita comportamiento automático de API REST
public class ProductController : ControllerBase
{
    private readonly IProductsManagementsService _service;

    public ProductController(IProductsManagementsService service)
    {
        _service = service;
    }

    [HttpPost] // POST /api/products → crea un nuevo producto
    public async Task<IActionResult> AddProduct([FromBody] ProductModelDto.ProductRequestWithDescription request)
    {
        var product = await _service.AddProduct(request);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [AllowAnonymous] // Cualquier persona puede consultar el catálogo
    [HttpGet]        // GET /api/products → obtiene todos los productos
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _service.GetAllProducts();
        if (products == null || !products.Any())
            return NoContent(); // No hay productos

        return Ok(products); // Lista de productos
    }

    [HttpGet("admin")]
    //[Authorize(Roles = "ADMIN")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModelDto.FilterProduct request)
    {
        var products = await _service.GetProducts(request);

        if (products == null || !products.ProductItems.Any())
        {
            Response.Headers.Append("X-Message", "No hay productos activos");
            return NoContent();
        }

        return Ok(products);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetProductCount()
    {
        var count = await _service.GetProductCountAsync();
        return Ok(count);
    }

    [AllowAnonymous] // Cualquier persona puede buscar un producto por ID
    [HttpGet("{id}")] // GET /api/products/{id} → obtiene un producto por su ID
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _service.GetProductById(id);
        if (product == null)
            return NotFound(); // No existe

        return Ok(product); // Producto encontrado

    }
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest("Debe ingresar un término de búsqueda");

        var products = await _service.SearchProducts(term);

        if (products == null || !products.Any())
            return NoContent();

        return Ok(products);
    }

    [HttpPut("{id}")] // PUT /api/products/{id} → actualiza un producto
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModelDto.ProductRequestWithDescription request)
    {
        var product = await _service.GetProductById(id);
        if (product == null)
            return NotFound(); // No existe

        await _service.ModifyProduct(product, request);
        return NoContent(); // Actualización exitosa sin contenido
    }

    [HttpPatch("{id}")] // PATCH /api/products/{id} → alterna estado activo/inactivo
    public async Task<IActionResult> PatchProductIsActive(Guid id)
    {
        var product = await _service.GetProductById(id);
        if (product == null)
            return NotFound(); // No existe

        await _service.PatchProductIsActive(product);
        return NoContent(); // Cambió el estado
    }
}