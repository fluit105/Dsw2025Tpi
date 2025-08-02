namespace Dsw2025Tpi.Domain.Entities;

// Base común de todas las entidades del dominio.
// Centraliza la identidad para mantener consistencia en toda la capa de dominio.
public abstract class EntityBase
{
      // Constructor protegido: se ejecuta al instanciar cualquier entidad derivada.
      // Genera un identificador único inmediato, de forma que cada entidad tenga su propia clave sin depender de la persistencia.
      protected EntityBase()
      {
            Id = Guid.NewGuid();
      }

      // Identificador inmutable de la entidad. Se inicializa una sola vez en el constructor y luego no cambia.
      public Guid Id { get; init; }
}
