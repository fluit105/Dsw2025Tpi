using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
      // Excepción lanzada cuando se intenta crear o actualizar
      // una entidad que ya existe y debe ser única.
      public class DuplicatedEntityException : Exception
      {
            public DuplicatedEntityException(string message) : base(message) { }
      }
}
