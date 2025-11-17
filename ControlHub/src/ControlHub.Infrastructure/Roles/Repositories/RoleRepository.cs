using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Roles;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Roles.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(AppDbContext db, ILogger<RoleRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                var entity = RoleMapper.ToEntity(role);
                await _db.Roles.AddAsync(entity, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add Role {Id}", role.Id);
                throw new RepositoryException("Error adding role to database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding Role {Id}", role.Id);
                throw new RepositoryException("Unexpected error while adding role.", ex);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken)
        {
            try
            {
                var entities = roles.Select(RoleMapper.ToEntity).ToList();
                await _db.Roles.AddRangeAsync(entities, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple roles.");
                throw new RepositoryException("Error adding multiple roles.", ex);
            }
        }

        public async Task UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                var entity = RoleMapper.ToEntity(role);
                _db.Roles.Update(entity);
                await Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while updating Role {Id}", role.Id);
                throw new RepositoryConcurrencyException("Concurrency conflict while updating role.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating Role {Id}", role.Id);
                throw new RepositoryException("Unexpected error while updating role.", ex);
            }
        }

        public async Task UpdateRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken)
        {
            try
            {
                var entities = roles.Select(RoleMapper.ToEntity).ToList();
                _db.Roles.UpdateRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple roles.");
                throw new RepositoryException("Error updating multiple roles.", ex);
            }
        }

        public async Task DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                var entity = RoleMapper.ToEntity(role);
                _db.Roles.Remove(entity);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Role {Id}", role.Id);
                throw new RepositoryException("Error deleting role.", ex);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken)
        {
            try
            {
                var entities = roles.Select(RoleMapper.ToEntity).ToList();
                _db.Roles.RemoveRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple roles.");
                throw new RepositoryException("Error deleting multiple roles.", ex);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during Role SaveChanges");
                throw new RepositoryConcurrencyException("Concurrency conflict while saving roles.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during Role SaveChanges");
                throw new RepositoryException("Database update error while saving roles.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Role SaveChanges");
                throw new RepositoryException("Unexpected error during role save operation.", ex);
            }
        }

        public async Task<Role> GetByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var entity = await _db.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            return RoleMapper.ToDomain(entity);
        }
    }
}
