using ControlHub.Application.OutBoxs.Repositories;
using ControlHub.Domain.Outboxs;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Outboxs.Repositories
{
    public class OutboxCommands : IOutboxCommands
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OutboxCommands> _logger;

        public OutboxCommands(AppDbContext db, ILogger<OutboxCommands> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(OutboxMessage domainOutbox, CancellationToken cancellationToken)
        {
            try
            {
                var entity = OutboxMapper.ToEntity(domainOutbox);
                await _db.OutboxMessages.AddAsync(entity, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add OutboxMessage {Id}", domainOutbox.Id);
                throw new RepositoryException("Error adding outbox message to database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding OutboxMessage {Id}", domainOutbox.Id);
                throw new RepositoryException("Unexpected error while adding outbox message.", ex);
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
                _logger.LogError(ex, "Concurrency conflict during Outbox SaveChanges");
                throw new RepositoryConcurrencyException("Concurrency conflict while saving outbox messages.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during Outbox SaveChanges");
                throw new RepositoryException("Database update error while saving outbox messages.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Outbox SaveChanges");
                throw new RepositoryException("Unexpected error during outbox save operation.", ex);
            }
        }
    }
}