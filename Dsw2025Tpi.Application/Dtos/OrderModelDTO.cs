using Dsw2025Tpi.Domain.Domain;
using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Application.Dtos;

// DTO maestro para manejo de órdenes en la API.
// Agrupa todos los sub-modelos necesarios para crear, actualizar, filtrar y devolver órdenes.
public record OrderModelDto
{
      // Petición para actualizar el estado de una orden.
      public record OrderStatusUpdateRequest(OrderStatus OrderStatus);

      // Petición para crear una nueva orden.
      // Incluye el cliente, direcciones y los productos solicitados.
      public record OrderRequest(
          Guid customerId,
          string shippingAddress,
          string billingAddress,
          List<ProductModelDto.ProductRequest> Products
      );

      // Petición para filtrar órdenes en consultas/listados.
      // Parámetros opcionales para estado, cliente y paginación.
      public record OrderFilterRequest(
          OrderStatus? OrderStatus,
          Guid? CustomerId,
          int? pageNumber,
          int? pagesize
      );

      // Respuesta que devuelve la API con información detallada de una orden.
      public record OrderResponse(
          Guid customerId,
          Guid OrderId,
          DateTime date,
          string shippingAddress,
          string billingAddress,
          string notes,
          decimal totalmount,
          List<OrderItemsModelDto.OrderItemResponse> OrderItems,
          OrderStatus OrderStatus
      );
}
