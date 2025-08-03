using System;

namespace Dsw2025Tpi.Application.Dtos;

// DTO para el inicio de sesión de un usuario.
// Se envía desde el cliente a la API con las credenciales.
public record LoginModel(
    // Nombre de usuario o identificador para iniciar sesión.
    string UserName,

    // Contraseña en texto plano que envía el cliente.
    // Será validada contra el hash almacenado por Identity.
    string Password
);
