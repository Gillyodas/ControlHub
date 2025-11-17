using ControlHub.Domain.Permissions;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Roles.Commands.SetRolePermissions
{
    public sealed record AddPermissonsForRoleCommand(string roleId,
        IEnumerable<string> permissionIds,
        CancellationToken cancellationToken) : IRequest<Result<PartialResult<Permission, string>>>;
}