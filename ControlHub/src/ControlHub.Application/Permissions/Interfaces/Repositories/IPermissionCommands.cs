using ControlHub.Domain.Permissions;

namespace ControlHub.Application.Permissions.Interfaces.Repositories
{
    public interface IPermissionCommands
    {
        Task AddAsync(Permission permission, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken);
        Task UpdateAsync(Permission permission, CancellationToken cancellationToken);
        Task UpdateRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken);
        Task DeleteAsync(Permission permission, CancellationToken cancellationToken);
        Task DeleteRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}