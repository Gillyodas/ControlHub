using ControlHub.API.Accounts.Mappers;
using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.API.Accounts.ViewModels.Response;
using ControlHub.Application.Accounts.Commands.RefreshAccessToken;
using ControlHub.Application.Accounts.Commands.SignOut;
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
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var command = RegisterUserRequestMapper.ToCommand(request);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new RegisterUserResponse { Message = result.Error.Message });

            return Ok(new RegisterUserResponse
            {
                AccountId = result.Value,
                Message = "Register success"
            });
        }

        [Authorize(Policy = "Permission:account.register_admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> Register([FromBody] RegisterAdminRequest request, CancellationToken cancellationToken)
        {
            var command = RegisterAdminRequestMapper.ToCommand(request);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new RegisterAdminResponse { Message = result.Error.Message });

            return Ok(new RegisterAdminResponse
            {
                AccountId = result.Value,
                Message = "Register success"
            });
        }

        [AllowAnonymous]
        [HttpPost("register-supperadmin")]
        public async Task<IActionResult> Register([FromBody] RegisterSupperAdminRequest request, CancellationToken cancellationToken)
        {
            var command = RegisterSupperAdminRequestMapper.ToCommand(request);

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new RegisterSupperAdminResponse { Message = result.Error.Message });

            return Ok(new RegisterSupperAdminResponse
            {
                AccountId = result.Value,
                Message = "Register success"
            });
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
        {
            var command = SignInRequestMapper.ToCommand(request);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new SignInResponse { message = result.Error.Message });

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
        public async Task<IActionResult> Refresh([FromBody] RefreshAccessTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new RefreshAccessTokenCommand(request.RefreshToken, request.AccID, request.AccessToken);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new SignInResponse { message = result.Error.Message });

            return Ok(new RefreshAccessTokenReponse
            {
                RefreshToken = request.RefreshToken,
                AccessToken = request.AccessToken
            });
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut([FromBody] SignOutRequest request, CancellationToken cancellationToken)
        {
            var command = new SignOutCommand(request.accessToken, request.refreshToken);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new SignOutResponse { message = result.Error.Message });

            return Ok(new SignOutResponse
            {
                message = null
            });
        }
    }
}
