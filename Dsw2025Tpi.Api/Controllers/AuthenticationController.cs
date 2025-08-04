using Dsw2025Tpi.Application.Dtos;        // Modelos DTO para login y registro
using Dsw2025Tpi.Application.Interfaces;  // Interfaces de servicios (JWT, gestión de clientes)
using Microsoft.AspNetCore.Identity;      // Identity para manejo de usuarios y autenticación
using Microsoft.AspNetCore.Mvc;           // MVC para definir controladores y endpoints

namespace Dsw2025Tpi.Api.Controllers;

// Controlador de autenticación.
// Expone endpoints para login y registro de usuarios.
// Se encuentra bajo la ruta base: /api/auth

// Indica que este controlador maneja peticiones HTTP y aplica
// validaciones automáticas de modelo
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
      // Servicios inyectados por el contenedor de dependencias

      // Maneja la creación, consulta y actualización de usuarios Identity
      private readonly UserManager<IdentityUserCustomerDto> _userManager;

      // Maneja el inicio de sesión y validación de credenciales
      private readonly SignInManager<IdentityUserCustomerDto> _signInManager;

      // Genera tokens JWT para autenticación
      private readonly IJwtTokenService _jwtTokenService;

      // Maneja la creación de clientes en el dominio
      private readonly ICustomerManagmentsService _customerManagementsService;

      // Constructor: recibe dependencias inyectadas por el framework
      public AuthenticationController(
          IJwtTokenService jwtTokenService,
          SignInManager<IdentityUserCustomerDto> signInManager,
          UserManager<IdentityUserCustomerDto> userManager,
          ICustomerManagmentsService customerManagementsService)
      {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _customerManagementsService = customerManagementsService;
      }

      [HttpPost("login")] // Indica que este endpoint responde a POST en /api/auth/login
      public async Task<IActionResult> Login([FromBody] LoginModelDto loginModel)
      {
            // Validar que el request tenga los campos requeridos
            if (loginModel == null || string.IsNullOrEmpty(loginModel.UserName) ||
                  string.IsNullOrEmpty(loginModel.Password))

                  return BadRequest("Invalid login request.");

            // Buscar usuario en la base de datos de Identity por su nombre de usuario
            var user = await _userManager.FindByNameAsync(loginModel.UserName);
            if (user == null)
                  return Unauthorized("Invalid username or password."); // Usuario no encontrado

            // Verificar que la contraseña proporcionada sea correcta
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);
            if (!result.Succeeded)
                  return Unauthorized("Invalid username or password."); // Contraseña incorrecta

            // Obtener el rol real del usuario
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            // Generar un token JWT para este usuario incluyendo el rol real
            var token = _jwtTokenService.GenerateToken(user.UserName!, role);

            // Responder con el token para que el cliente lo use en futuras peticiones protegidas
            return Ok(new { Token = token });
      }

      [HttpPost("register")] // Indica que este endpoint responde a POST en /api/auth/register
      public async Task<IActionResult> Register([FromBody] RegisterModelDto registerModel)
      {
            // Validar que el request tenga usuario y contraseña
            if (registerModel == null || string.IsNullOrEmpty(registerModel.UserName) || 
                  string.IsNullOrEmpty(registerModel.Password))

                  return BadRequest("Invalid registration request.");

            // Crear un nuevo cliente en el dominio usando el servicio de gestión de clientes
            // Esto guarda el cliente en la base de datos de dominio y devuelve la entidad creada
            var customer = await _customerManagementsService.CreateCustomerAsync(
                registerModel.Email,
                registerModel.Name,
                registerModel.PhoneNumber
            );

            // Crear un nuevo usuario Identity vinculado al cliente recién creado
            var user = new IdentityUserCustomerDto
            {
                  UserName = registerModel.UserName,
                  Email = registerModel.Email,
                  CustomerId = customer.Id
            };

            // Crear el usuario en la base de datos de Identity con la contraseña especificada
            var result = await _userManager.CreateAsync(user, registerModel.Password);

            // Si la creación falla, devolver los errores
            if (!result.Succeeded)
                  return BadRequest(result.Errors);

            // Asignar rol por defecto al usuario recién registrado
            // Esto asegura que en el login podamos devolver el rol real en el token
            await _userManager.AddToRoleAsync(user, "Customer");

            // Si todas las cosas salen bien, devolverá mensaje de éxito
            return Ok("User registered successfully.");
      }
}
