using ControlHub.API.Roles.ViewModels.Requests;
using ControlHub.API.Roles.ViewModels.Responses;
using ControlHub.Application.Roles.Commands.CreateRoles;
using ControlHub.Application.Roles.Commands.SetRolePermissions;
using ControlHub.Domain.Permissions;
using ControlHub.Domain.Roles;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlHub.API.Roles
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        //[Authorize(Policy = "Permission:role.add_permissions")]
        [HttpPost("update")]
        public async Task<IActionResult> AddPermissionsForRole([FromBody] AddPermissonsForRoleRequest request, CancellationToken cancellationToken)
        {
            var command = new AddPermissonsForRoleCommand(request.RoleId, request.PermissionIds, cancellationToken);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new AddPermissonsForRoleResponse
                {
                    Message = result.Error.Message
                });
            }

            if (result is Result<PartialResult<Permission, string>> typedResult)
            {
                var summary = typedResult.Value;
                return Ok(new AddPermissonsForRoleResponse
                {
                    Message = summary.Failures.Any()
                        ? "Partial success: some permissions failed to add."
                        : "All permissions add successfully.",
                    SuccessCount = summary.Successes.Count(),
                    FailureCount = summary.Failures.Count(),
                    FailedRoles = summary.Failures
                });
            }

            // fallback – nếu handler chỉ trả về Result.Success()
            return Ok(new AddPermissonsForRoleResponse
            {
                Message = "All roles created successfully.",
                SuccessCount = request.PermissionIds.Count(),
                FailureCount = 0
            });
        }

        [Authorize(Policy = "Permission:role.create")]
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRoles([FromBody] CreateRolesRequest request, CancellationToken ct)
        {
            var command = new CreateRolesCommand(request.Roles);
            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
            {

                return BadRequest(new CreateRolesResponse
                {
                    Message = result.Error.Message,
                    SuccessCount = 0,
                    FailureCount = request.Roles.Count()
                });
            }

            if (result is Result<PartialResult<Role, string>> typedResult)
            {
                var summary = typedResult.Value;
                return Ok(new CreateRolesResponse
                {
                    Message = summary.Failures.Any()
                        ? "Partial success: some roles failed to create."
                        : "All roles created successfully.",
                    SuccessCount = summary.Successes.Count(),
                    FailureCount = summary.Failures.Count(),
                    FailedRoles = summary.Failures
                });
            }

            // fallback – nếu handler chỉ trả về Result.Success()
            return Ok(new CreateRolesResponse
            {
                Message = "All roles created successfully.",
                SuccessCount = request.Roles.Count(),
                FailureCount = 0
            });
        }
    }
}
