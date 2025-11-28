using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Roles;
using ControlHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.Roles.Repositories
{
    public class RoleQueries : IRoleQueries
    {
        private readonly AppDbContext _db;

        public RoleQueries(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _db.Roles
                .AsNoTracking()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _db.Roles
                .AsNoTracking()
                .Include(r => r.Permissions)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Role>> SearchByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _db.Roles
                .AsNoTracking()
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .Include(r => r.Permissions)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistAsync(Guid roleId, CancellationToken cancellationToken)
        {
            return await _db.Roles
                .AsNoTracking()
                .AnyAsync(r => r.Id == roleId, cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            return await _db.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);
        }
    }
}