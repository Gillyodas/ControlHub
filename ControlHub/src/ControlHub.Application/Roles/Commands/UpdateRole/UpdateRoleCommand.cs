using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Roles.Commands.UpdateRole
{
    public record UpdateRoleCommand(
        Guid Id,
        string Name,
        string Description
    ) : IRequest<Result<Unit>>;
}
