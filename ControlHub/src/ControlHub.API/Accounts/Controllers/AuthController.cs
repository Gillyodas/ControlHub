using ControlHub.API.Accounts.Mappers;
using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.API.Accounts.ViewModels.Response;
using ControlHub.Application.Accounts.Commands.RefreshAccessToken;
using ControlHub.Application.Accounts.Commands.SignOut;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            var command = RegisterUserRequestMapper.ToCommand(request);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result); // Tự động map lỗi

            return Ok(new RegisterUserResponse
            {
                AccountId = result.Value,
                Message = "Register success"
            });
        }

        [Authorize(Policy = "Permission:account.register_admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request, CancellationToken ct)
        {
            var command = RegisterAdminRequestMapper.ToCommand(request);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result);

            return Ok(new RegisterAdminResponse
            {
                AccountId = result.Value,
                Message = "Admin registration success"
            });
        }

        [AllowAnonymous] // Đã sửa thành AllowAnonymous và check MasterKey trong Handler
        [HttpPost("register-superadmin")]
        public async Task<IActionResult> RegisterSuperAdmin([FromBody] RegisterSupperAdminRequest request, CancellationToken ct)
        {
            var command = RegisterSupperAdminRequestMapper.ToCommand(request);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result); // Sẽ trả về 403 nếu sai MasterKey

            return Ok(new RegisterSupperAdminResponse
            {
                AccountId = result.Value,
                Message = "SuperAdmin registration success"
            });
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        [ProducesResponseType(typeof(SignInResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken ct)
        {
            var command = SignInRequestMapper.ToCommand(request);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result); // Sẽ trả về 401 nếu sai pass/user

            return Ok(new SignInResponse
            {
                accountId = result.Value.AccountId,
                username = result.Value.Username,
                accessToken = result.Value.AccessToken,
                refreshToken = result.Value.RefreshToken
            });
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshAccessTokenRequest request, CancellationToken ct)
        {
            var command = new RefreshAccessTokenCommand(request.RefreshToken, request.AccID, request.AccessToken);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result);

            return Ok(new RefreshAccessTokenReponse
            {
                RefreshToken = request.RefreshToken,
                AccessToken = request.AccessToken
            });
        }

        [Authorize]
        [HttpPost("signout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SignOut([FromBody] SignOutRequest request, CancellationToken ct)
        {
            var command = new SignOutCommand(request.accessToken, request.refreshToken);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return HandleFailure(result);

            return NoContent();
        }

        private IActionResult HandleFailure(Result result)
        {
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(CreateProblemDetails("Validation Error", StatusCodes.Status400BadRequest, result.Error)),
                ErrorType.NotFound => NotFound(CreateProblemDetails("Resource Not Found", StatusCodes.Status404NotFound, result.Error)),
                ErrorType.Conflict => Conflict(CreateProblemDetails("Conflict", StatusCodes.Status409Conflict, result.Error)),
                ErrorType.Unauthorized => Unauthorized(CreateProblemDetails("Unauthorized", StatusCodes.Status401Unauthorized, result.Error)),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails("Forbidden", StatusCodes.Status403Forbidden, result.Error)),
                _ => BadRequest(CreateProblemDetails("Bad Request", StatusCodes.Status400BadRequest, result.Error)) // Mặc định
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
