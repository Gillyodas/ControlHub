using ControlHub.API.Roles.ViewModels.Requests;
using ControlHub.API.Roles.ViewModels.Responses;
using ControlHub.Application.Roles.Commands.CreateRoles;
using MediatR;
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
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRoles([FromBody] CreateRolesRequest request)
        {
            var command = new CreateRolesCommand(request.Roles);

            var result = await _mediator.Send(command);

            if(result.IsFailure)
            {
                return BadRequest(new CreateRolesResponse { Message = result.Error.Message });
            }

            return Ok();
        }
    }
}
