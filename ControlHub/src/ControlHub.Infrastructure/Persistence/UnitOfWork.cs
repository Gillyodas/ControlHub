using ControlHub.Application.Common.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<int> CommitAsync(CancellationToken ct = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                _logger.LogInformation("Transaction started at {Time}", DateTime.UtcNow);

                var changes = await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation("Transaction committed successfully with {Changes} changes.", changes);
                return changes;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency conflict during transaction. Rolling back changes...");

                await SafeRollbackAsync(transaction, ct);
                _dbContext.ChangeTracker.Clear();

                throw new RepositoryConcurrencyException(
                    "A concurrency conflict occurred while committing the transaction.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Database update error during transaction. Rolling back changes...");

                await SafeRollbackAsync(transaction, ct);
                _dbContext.ChangeTracker.Clear();

                throw new RepositoryException(
                    "A database update error occurred while committing the transaction.", ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex,
                    "Transaction was cancelled. Rolling back changes...");

                await SafeRollbackAsync(transaction, ct);
                _dbContext.ChangeTracker.Clear();

                throw new RepositoryException("Transaction was cancelled by request.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Unexpected error during transaction. Rolling back changes...");

                await SafeRollbackAsync(transaction, ct);
                _dbContext.ChangeTracker.Clear();

                throw new RepositoryException(
                    "An unexpected error occurred during transaction commit.", ex);
            }
        }

        private async Task SafeRollbackAsync(IDbContextTransaction transaction, CancellationToken ct)
        {
            try
            {
                await transaction.RollbackAsync(ct);
                _logger.LogWarning("Transaction rolled back at {Time}.", DateTime.UtcNow);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogCritical(rollbackEx,
                    "Rollback failed. Manual intervention may be required.");
            }
        }
    }
}