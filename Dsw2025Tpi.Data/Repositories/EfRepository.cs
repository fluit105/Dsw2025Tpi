using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Data.Repositories;

// Implementación del repositorio genérico usando Entity Framework Core
public class EfRepository : IRepository
{
      private readonly DomainContext _context; // Contexto de base de datos

      public EfRepository(DomainContext context)
      {
            _context = context;
      }

      // Agrega una nueva entidad a la base de datos
      public async Task<T> Add<T>(T entity) where T : EntityBase
      {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
      }

      // Elimina una entidad existente
      public async Task<T> Delete<T>(T entity) where T : EntityBase
      {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
      }

      // Devuelve el primer elemento que cumpla una condición
      public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
      {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
      }

      // Devuelve todas las entidades de un tipo
      public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
      {
            return await Include(_context.Set<T>(), include).ToListAsync();
      }

      // Busca una entidad por su Id
      public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
      {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
      }

      // Devuelve las entidades que cumplan un filtro
      public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
      {
            return await Include(_context.Set<T>(), include).Where(predicate).ToListAsync();
      }

      // Actualiza una entidad existente
      public async Task<T> Update<T>(T entity) where T : EntityBase
      {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
      }

      // Aplica Includes para cargar propiedades relacionadas
      private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
      {
            var includedQuery = query;

            foreach (var include in includes)
            {
                  includedQuery = includedQuery.Include(include);
            }
            return includedQuery;
      }
}
