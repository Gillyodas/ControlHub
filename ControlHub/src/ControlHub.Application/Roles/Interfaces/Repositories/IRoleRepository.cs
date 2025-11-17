using ControlHub.Domain.Roles;

namespace ControlHub.Application.Roles.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task AddAsync(Role role, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken);
        Task UpdateAsync(Role role, CancellationToken cancellationToken);
        Task UpdateRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken);
        Task DeleteAsync(Role role, CancellationToken cancellationToken);
        Task DeleteRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);

        //Query for commands
        Task<Role> GetByIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
