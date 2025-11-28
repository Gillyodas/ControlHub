using ControlHub.API.Accounts.Mappers;
using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.API.Accounts.ViewModels.Response;
using ControlHub.Application.Accounts.Commands.ChangePassword;
using ControlHub.Application.Accounts.Commands.ResetPassword;
using ControlHub.Infrastructure.Authorization.Requirements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class accountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthorizationService _authorizationService;

        public accountController(IMediator mediator, IAuthorizationService authorizationService)
        {
            _mediator = mediator;
            _authorizationService = authorizationService;
        }

        [Authorize]
        [HttpPost("change-password/{id}")]
        public async Task<IActionResult> ChangePasswordCommand(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, new SameUserRequirement());

            if (!authResult.Succeeded)
            {
                return Forbid(); // Trả về 403
            }
            var command = new ChangePasswordCommand(id, request.curPass, request.newPass);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new ChangePasswordResponse { Message = result.Error.Message });

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = ForgotPasswordRequestMapper.ToCommand(request);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new ForgotPasswordResponse { Message = result.Error.Message });

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ResetPasswordCommand(request.Token, request.Password);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new ResetPasswordResponse { Message = result.Error.Message });

            return Ok();
        }
    }
}
