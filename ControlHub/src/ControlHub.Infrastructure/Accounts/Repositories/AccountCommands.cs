using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Domain.Accounts;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Accounts.Repositories
{
    public class AccountCommands : IAccountCommands
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AccountCommands> _logger;

        public AccountCommands(AppDbContext db, ILogger<AccountCommands> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(Account accDomain, CancellationToken cancellationToken)
        {
            try
            {
                var accEntity = AccountMapper.ToEntity(accDomain);
                await _db.Accounts.AddAsync(accEntity, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add Account {Id}", accDomain.Id);
                throw new RepositoryException("Error adding account to database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding Account {Id}", accDomain.Id);
                throw new RepositoryException("Unexpected error while adding account.", ex);
            }
        }

        public async Task UpdateAsync(Account accDomain, CancellationToken cancellationToken)
        {
            try
            {
                var accEntity = AccountMapper.ToEntity(accDomain);
                _db.Accounts.Update(accEntity);
                await Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while updating Account {Id}", accDomain.Id);
                throw new RepositoryConcurrencyException("Concurrency conflict while updating account.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating Account {Id}", accDomain.Id);
                throw new RepositoryException("Database error while updating account.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating Account {Id}", accDomain.Id);
                throw new RepositoryException("Unexpected error while updating account.", ex);
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
                _logger.LogError(ex, "Concurrency conflict during Account SaveChanges");
                throw new RepositoryConcurrencyException("Concurrency conflict while saving accounts.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during Account SaveChanges");
                throw new RepositoryException("Database update error while saving accounts.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Account SaveChanges");
                throw new RepositoryException("Unexpected error during account save operation.", ex);
            }
        }
    }
}