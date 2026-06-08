using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;
using System.Collections.Generic;
using System.Net;

namespace SolicitudServidores.Utilities
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // Buscar PostgresException en la cadena de excepciones
            var pgEx = FindPostgresException(exception);
            if (pgEx != null)
            {
                var (code, message) = pgEx.SqlState switch
                {
                    "23505" => (HttpStatusCode.Conflict,         "Ya existe un registro con ese valor (clave duplicada)."),
                    "23503" => (HttpStatusCode.BadRequest,       "El registro referenciado no existe (violación de llave foránea)."),
                    "23502" => (HttpStatusCode.BadRequest,       "Un campo requerido no puede ser nulo."),
                    _       => (HttpStatusCode.InternalServerError, pgEx.Message)
                };

                context.Result = new ObjectResult(new { statusCode = (int)code, message })
                {
                    StatusCode = (int)code
                };
                context.ExceptionHandled = true;
                return;
            }

            var statusCode = exception switch
            {
                KeyNotFoundException      => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException         => HttpStatusCode.BadRequest,
                _                         => HttpStatusCode.InternalServerError
            };

            context.Result = new ObjectResult(new
            {
                statusCode = (int)statusCode,
                message    = exception.Message,
                details    = exception.InnerException?.Message
            })
            {
                StatusCode = (int)statusCode
            };

            context.ExceptionHandled = true;
        }

        private static PostgresException? FindPostgresException(Exception ex)
        {
            if (ex is PostgresException pg) return pg;
            if (ex.InnerException != null) return FindPostgresException(ex.InnerException);
            return null;
        }
    }
}
