using ControlHub.Application.Users.Interfaces.Repositories;
using ControlHub.Domain.Users;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Users.Repositories
{
    public class UserCommands : IUserCommands
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UserCommands> _logger;

        public UserCommands(AppDbContext db, ILogger<UserCommands> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var userEntity = UserMapper.ToEntity(user);
                await _db.Users.AddAsync(userEntity, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add User {Id}", user.Id);
                throw new RepositoryException("Error adding user to database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding User {Id}", user.Id);
                throw new RepositoryException("Unexpected error while adding user.", ex);
            }
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var userEntity = UserMapper.ToEntity(user);
                _db.Users.Update(userEntity);
                await Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while updating User {Id}", user.Id);
                throw new RepositoryConcurrencyException("Concurrency conflict while updating user.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating User {Id}", user.Id);
                throw new RepositoryException("Database error while updating user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating User {Id}", user.Id);
                throw new RepositoryException("Unexpected error while updating user.", ex);
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
                _logger.LogError(ex, "Concurrency conflict during User SaveChanges");
                throw new RepositoryConcurrencyException("Concurrency conflict while saving users.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during User SaveChanges");
                throw new RepositoryException("Database update error while saving users.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during User SaveChanges");
                throw new RepositoryException("Unexpected error during user save operation.", ex);
            }
        }
    }
}