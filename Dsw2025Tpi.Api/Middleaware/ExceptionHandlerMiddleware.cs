using System.Net;
using System.Text.Json;
using Dsw2025Tpi.Application.Exceptions;   // usa mis excepciones

namespace Dsw2025Tpi.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // continúa con el pipeline
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // captura global
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            // Mapeo ajustado de mis excepciones
            var statusCode = exception switch
            {
                // Argumentos/datos inválidos
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,

                // TUS excepciones de dominio/aplicación
                DuplicatedEntityException => HttpStatusCode.Conflict,             // 409
                NotExistException => HttpStatusCode.NotFound,                     // 404
                NotExistOrderStatusException => HttpStatusCode.BadRequest,        // 400
                OrderEmptyException => HttpStatusCode.UnprocessableEntity,        // 422
                StockInsufficientException => HttpStatusCode.UnprocessableEntity, // 422

                // Fallback
                _ => HttpStatusCode.InternalServerError                           // 500
            };

            // Respuesta JSON consistente (incluye traceId para correlación)
            var payload = new
            {
                error = exception.Message,
                type = exception.GetType().Name,
                status = (int)statusCode,
                traceId = context.TraceIdentifier
            };

            response.StatusCode = (int)statusCode;
            return response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}