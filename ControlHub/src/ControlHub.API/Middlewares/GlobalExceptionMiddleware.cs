using System.Net;
using System.Text.Json;

namespace ControlHub.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // cho request chạy tiếp
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception at {Path} with TraceId {TraceId}",
                    context.Request.Path,
                    context.TraceIdentifier);

                var errorResponse = new
                {
                    code = "System.UnhandledException",
                    message = "Unexpected error occurred",
                    traceId = context.TraceIdentifier
                };

                int statusCode = ex switch
                {
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.InternalServerError
                };
                context.Response.StatusCode = statusCode;

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}