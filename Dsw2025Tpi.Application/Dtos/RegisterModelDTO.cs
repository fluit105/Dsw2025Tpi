using System;

namespace Dsw2025Tpi.Application.Dtos;

// DTO para registrar un nuevo usuario/cliente en el sistema.
// Se envía desde el cliente a la API con las credenciales y datos básicos.
public record RegisterModelDto(
    string UserName,

    string Password,

    string Email,

    string Name,

    string PhoneNumber
);
