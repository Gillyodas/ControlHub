using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.API.Accounts.ViewModels.Response;
using ControlHub.API.Controllers;
using ControlHub.Application.Accounts.Commands.ChangePassword;
using ControlHub.Application.Accounts.Commands.ForgotPassword;
using ControlHub.Application.Accounts.Commands.ResetPassword;
using ControlHub.Infrastructure.Authorization.Requirements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Accounts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseApiController
    {
        private readonly IAuthorizationService _authorizationService;
        public AccountController(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator)
        {
            _authorizationService = authorizationService;
        }

        [Authorize]
        [HttpPost("change-password/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            // Resource-based Authorization: Kiểm tra quyền trên tài nguyên cụ thể (id)
            var authResult = await _authorizationService.AuthorizeAsync(User, id, new SameUserRequirement());

            if (!authResult.Succeeded)
            {
                return Forbid(); // Trả về 403 Forbidden chuẩn xác
            }

            var command = new ChangePasswordCommand(id, request.curPass, request.newPass);

            var result = await Mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return HandleFailure(result); // Tự động map lỗi (VD: Sai pass cũ -> 400, Account Deleted -> 400)
            }

            return NoContent(); // Thành công và không có nội dung trả về
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ForgotPasswordCommand(request.Value, request.Type);

            var result = await Mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ResetPasswordCommand(request.Token, request.Password);

            var result = await Mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok();
        }
    }
}