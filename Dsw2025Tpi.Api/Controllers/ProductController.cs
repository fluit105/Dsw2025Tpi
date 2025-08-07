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
            try
            {
                  var product = await _service.AddProduct(request);
                  return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException ae) // Datos inválidos
            {
                  return BadRequest(ae.Message);
            }
            catch (DuplicatedEntityException de) // SKU duplicado
            {
                  return Conflict(de.Message);
            }
            catch (Exception) // Error inesperado
            {
                  return Problem("An error occurred while saving the product");
            }
      }

      [AllowAnonymous] // Cualquier persona puede consultar el catálogo
      [HttpGet]        // GET /api/products → obtiene todos los productos
      public async Task<IActionResult> GetAllProducts()
      {
            try
            {
                  var products = await _service.GetAllProducts();
                  if (products == null || !products.Any())
                        return NoContent(); // No hay productos

                  return Ok(products); // Lista de productos
            }
            catch (Exception)
            {
                  return Problem("An error occurred while retrieving the products");
            }
      }

      [AllowAnonymous] // Cualquier persona puede buscar un producto por ID
      [HttpGet("{id}")] // GET /api/products/{id} → obtiene un producto por su ID
      public async Task<IActionResult> GetProductById(Guid id)
      {
            try
            {
                  var product = await _service.GetProductById(id);
                  if (product == null)
                        return NotFound(); // No existe

                  return Ok(product); // Producto encontrado
            }
            catch (Exception)
            {
                  return Problem("An error occurred while retrieving the product");
            }
      }

      [HttpPut("{id}")] // PUT /api/products/{id} → actualiza un producto
      public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModelDto.ProductRequestWithDescription request)
      {
            try
            {
                  var product = await _service.GetProductById(id);
                  if (product == null)
                        return NotFound(); // No existe

                  await _service.ModifyProduct(product, request);
                  return NoContent(); // Actualización exitosa sin contenido
            }
            catch (Exception)
            {
                  return Problem("An error occurred while updating the product");
            }
      }

      [HttpPatch("{id}")] // PATCH /api/products/{id} → alterna estado activo/inactivo
      public async Task<IActionResult> PatchProductIsActive(Guid id)
      {
            try
            {
                  var product = await _service.GetProductById(id);
                  if (product == null)
                        return NotFound(); // No existe

                  await _service.PatchProductIsActive(product);
                  return NoContent(); // Cambió el estado
            }
            catch (Exception)
            {
                  return Problem("An error occurred while patching the product");
            }
      }
}
