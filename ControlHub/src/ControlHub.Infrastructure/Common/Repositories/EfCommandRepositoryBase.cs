using ControlHub.Application.Common.Repositories;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Common.Repositories
{
    public class EfCommandRepositoryBase<T> : ICommandRepositoryBase<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EfCommandRepositoryBase<T>> _logger;

        public EfCommandRepositoryBase(AppDbContext context, ILogger<EfCommandRepositoryBase<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            try
            {
                await _context.AddAsync(entity, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to add entity.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddAsync for {EntityType}", typeof(T).Name);
                throw new RepositoryException("Unexpected repository error.", ex);
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            try
            {
                await _context.AddRangeAsync(entities, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding entities of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to add multiple entities.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddRangeAsync for {EntityType}", typeof(T).Name);
                throw new RepositoryException("Unexpected repository error.", ex);
            }
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken)
        {
            try
            {
                _context.Remove(entity);
                return Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error deleting {EntityType}", typeof(T).Name);
                throw new RepositoryConcurrencyException("Optimistic concurrency failure on delete.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to delete entity.", ex);
            }
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            try
            {
                _context.RemoveRange(entities);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting multiple {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to delete multiple entities.", ex);
            }
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken)
        {
            try
            {
                _context.Update(entity);
                return Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating {EntityType}", typeof(T).Name);
                throw new RepositoryConcurrencyException("Optimistic concurrency failure on update.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to update entity.", ex);
            }
        }

        public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            try
            {
                _context.UpdateRange(entities);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating multiple {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to update multiple entities.", ex);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error during SaveChanges for {EntityType}", typeof(T).Name);
                throw new RepositoryConcurrencyException("Optimistic concurrency error on SaveChanges.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during SaveChanges for {EntityType}", typeof(T).Name);
                throw new RepositoryException("Database update failed.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SaveChanges for {EntityType}", typeof(T).Name);
                throw new RepositoryException("Unexpected repository error during SaveChanges.", ex);
            }
        }
    }
}
