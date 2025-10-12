using ControlHub.Domain.Outboxs;

namespace ControlHub.Application.OutBoxs.Repositories
{
    public interface IOutboxCommands
    {
        Task AddAsync(OutboxMessage domainOutbox, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
