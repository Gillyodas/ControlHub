using ControlHub.Domain.Roles;

namespace ControlHub.Application.Roles.Interfaces.Repositories
{
    public interface IRolePermissionsCommands
    {
        Task AddRangeAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken);
    }
}
