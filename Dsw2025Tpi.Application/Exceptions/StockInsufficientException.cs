using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
      // Excepción lanzada cuando se intenta crear una orden
      // y no hay stock suficiente para alguno de los productos.
      public class StockInsufficientException : Exception
      {
            public StockInsufficientException(string? message) : base(message) { }
      }
}
