using ControlHub.API.Controllers;
using ControlHub.API.Permissions.ViewModels.Requests;
using ControlHub.API.Permissions.ViewModels.Responses;
using ControlHub.Application.Permissions.Commands.CreatePermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Permissions
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : BaseApiController
    {
        public PermissionController(IMediator mediator) : base(mediator)
        {
        }

        [Authorize(Policy = "Permission:permission.create")]
        [HttpPost("permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreatePermissions([FromBody] CreatePermissionsRequest request, CancellationToken cancellationToken)
        {
            var command = new CreatePermissionsCommand(request.Permissions);

            var result = await Mediator.Send(command, cancellationToken); // Truyền cancellationToken

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok();
        }
    }
}