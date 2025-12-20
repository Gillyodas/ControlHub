using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly IMediator Mediator;

        protected BaseApiController(IMediator mediator)
        {
            Mediator = mediator;
        }

        /// <summary>
        /// Tự động ánh xạ từ Domain Error sang HTTP Status Code chuẩn
        /// </summary>
        protected IActionResult HandleFailure(Result result)
        {
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(CreateProblemDetails("Validation Error", StatusCodes.Status400BadRequest, result.Error)),
                ErrorType.NotFound => NotFound(CreateProblemDetails("Resource Not Found", StatusCodes.Status404NotFound, result.Error)),
                ErrorType.Conflict => Conflict(CreateProblemDetails("Conflict", StatusCodes.Status409Conflict, result.Error)),
                ErrorType.Unauthorized => Unauthorized(CreateProblemDetails("Unauthorized", StatusCodes.Status401Unauthorized, result.Error)),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails("Forbidden", StatusCodes.Status403Forbidden, result.Error)),
                _ => BadRequest(CreateProblemDetails("Bad Request", StatusCodes.Status400BadRequest, result.Error))
            };
        }

        private ProblemDetails CreateProblemDetails(string title, int status, Error error)
        {
            return new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = error.Message,
                Extensions = { { "code", error.Code } }
            };
        }
    }
}