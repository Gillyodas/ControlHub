using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.Domain.Permissions;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Permissions.Repositories
{
    public class PermissionCommands : IPermissionCommands
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PermissionCommands> _logger;

        public PermissionCommands(AppDbContext db, ILogger<PermissionCommands> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(Permission permission, CancellationToken cancellationToken)
        {
            try
            {
                var entity = PermissionMapper.ToEntity(permission);
                await _db.Permissions.AddAsync(entity, cancellationToken);
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
                var entities = permissions.Select(PermissionMapper.ToEntity).ToList();
                await _db.Permissions.AddRangeAsync(entities, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add multiple Permissions");
                throw new RepositoryException("Error adding multiple permissions.", ex);
            }
        }

        public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken)
        {
            try
            {
                var entity = PermissionMapper.ToEntity(permission);
                _db.Permissions.Update(entity);
                await Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new RepositoryConcurrencyException("Concurrency conflict while updating permission.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Permission {Id}", permission.Id);
                throw new RepositoryException("Unexpected error while updating permission.", ex);
            }
        }

        public async Task UpdateRangeAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken)
        {
            try
            {
                var entities = permissions.Select(PermissionMapper.ToEntity).ToList();
                _db.Permissions.UpdateRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple permissions");
                throw new RepositoryException("Unexpected error updating multiple permissions.", ex);
            }
        }

        public async Task DeleteAsync(Permission permission, CancellationToken cancellationToken)
        {
            try
            {
                var entity = PermissionMapper.ToEntity(permission);
                _db.Permissions.Remove(entity);
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
                var entities = permissions.Select(PermissionMapper.ToEntity).ToList();
                _db.Permissions.RemoveRange(entities);
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
    }
}