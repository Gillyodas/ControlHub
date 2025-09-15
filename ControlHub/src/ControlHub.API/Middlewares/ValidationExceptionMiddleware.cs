using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Common.Errors;

namespace ControlHub.API.Middlewares
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<IErrorCatalog> _catalogs;

        public ValidationExceptionMiddleware(RequestDelegate next, IEnumerable<IErrorCatalog> catalogs)
        {
            _next = next;
            _catalogs = catalogs;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = ex.Errors
                    .Select(e => new
                    {
                        Code = MapErrorCode(e.ErrorMessage),
                        Message = e.ErrorMessage
                    });

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new { errors });
            }
        }

        private string MapErrorCode(string errorMessage)
        {
            foreach (var catalog in _catalogs)
            {
                var code = catalog.GetCodeByMessage(errorMessage);
                if (code != null) return code;
            }
            return "Validation.Unknown";
        }
    }
}
