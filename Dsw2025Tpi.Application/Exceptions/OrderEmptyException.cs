using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
      // Excepción lanzada cuando se intenta crear una orden sin productos.
      public class OrderEmptyException : Exception
      {
            public OrderEmptyException(string? message) : base(message) { }
      }
}
