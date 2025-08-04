using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
      // Excepción lanzada cuando se intenta acceder a una entidad
      // que no existe en la base de datos.
      public class NotExistException : Exception
      {
            public NotExistException(string? message) : base(message) { }
      }
}
