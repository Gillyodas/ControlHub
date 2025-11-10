using ControlHub.API.Permissions.ViewModels.Requests;
using ControlHub.API.Permissions.ViewModels.Responses;
using ControlHub.Application.Permissions.Commands.CreatePermissions;
using ControlHub.Application.Permissions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Permissions
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        public PermissionController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }
        [Authorize(Policy = "Permission:permission.create")]
        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermissions([FromBody] CreatePermissionsRequest request, CancellationToken cancellationToken)
        {
            var command = new CreatePermissionsCommand(request.Permissions);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new CreatePermissionsResponse { Message = result.Error.Message });
            }

            return Ok();
        }
        [AllowAnonymous]
        [HttpGet("permissions/{roleId}")]
        public async Task<IActionResult> GetPermissionForRoleId(string roleId, CancellationToken cancellationToken)
        {
            var result = await _permissionService.GetPermissionsForRoleIdAsync(Guid.Parse(roleId), cancellationToken);
            return Ok(result);
        }
    }
}
