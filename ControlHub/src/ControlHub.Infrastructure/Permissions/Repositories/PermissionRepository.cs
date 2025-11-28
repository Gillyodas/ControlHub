using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.Domain.Permissions;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Permissions.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PermissionRepository> _logger;

        public PermissionRepository(AppDbContext db, ILogger<PermissionRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(Permission permission, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Permissions.AddAsync(permission, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add Permission {Id}", permission.Id);
                throw new RepositoryException("Error adding permission.", ex);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Permissions.AddRangeAsync(permissions, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add multiple Permissions");
                throw new RepositoryException("Error adding multiple permissions.", ex);
            }
        }

        public async Task DeleteAsync(Permission permission, CancellationToken cancellationToken)
        {
            try
            {
                _db.Permissions.Remove(permission);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Permission {Id}", permission.Id);
                throw new RepositoryException("Unexpected error deleting permission.", ex);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken)
        {
            try
            {
                _db.Permissions.RemoveRange(permissions);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple permissions");
                throw new RepositoryException("Unexpected error deleting multiple permissions.", ex);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Permission changes");
                throw new RepositoryException("Unexpected error saving permission changes.", ex);
            }
        }

        public async Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken)
        {
            return await _db.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }
    }
}