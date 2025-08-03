using Dsw2025Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Domain.Interfaces;

// Contrato genérico de acceso a datos para las entidades del dominio.
// Permite desacoplar la lógica de negocio / aplicación de la persistencia concreta.
public interface IRepository
{
      // Recupera por Id.
      Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase;

      // Devuelve el primer elemento que cumpla un predicado.
      Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

      // Recupera todos los elementos que coincidan con una condición dada.
      Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

      // Inserta una nueva entidad en el almacenamiento.
      Task<T> Add<T>(T entity) where T : EntityBase;

      // Actualiza una entidad existente.
      Task<T> Update<T>(T entity) where T : EntityBase;

      // Elimina una entidad.
      Task<T> Delete<T>(T entity) where T : EntityBase;
}
