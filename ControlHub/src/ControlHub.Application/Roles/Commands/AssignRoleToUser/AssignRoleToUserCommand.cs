using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Roles.Commands.AssignRoleToUser
{
    public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Result<Unit>>;
}
