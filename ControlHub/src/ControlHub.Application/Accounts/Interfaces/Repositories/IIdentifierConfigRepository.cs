using ControlHub.Domain.Accounts.Identifiers;

namespace ControlHub.Application.Accounts.Interfaces.Repositories
{
    public interface IIdentifierConfigRepository
    {
        Task<IdentifierConfig?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IdentifierConfig?> GetByNameAsync(string name, CancellationToken ct);
        Task<IEnumerable<IdentifierConfig>> GetActiveConfigsAsync(CancellationToken ct);
        Task AddAsync(IdentifierConfig config, CancellationToken ct);
    }
}
