using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Roles.Commands.SetRolePermissions
{
    public sealed record SetRolePermissionsCommand(Guid RoleId, List<Guid> PermissionIds) : IRequest<Result>;
}
