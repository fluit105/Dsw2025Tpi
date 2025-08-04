using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
      // Excepción lanzada cuando se intenta asignar un estado
      // que no existe dentro de los valores válidos definidos (ej. enum OrderStatus).
      public class NotExistOrderStatusException : Exception
      {
            public NotExistOrderStatusException(string message) : base(message) { }
      }
}
