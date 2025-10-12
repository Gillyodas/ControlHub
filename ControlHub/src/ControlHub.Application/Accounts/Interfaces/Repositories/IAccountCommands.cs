using ControlHub.Domain.Accounts;
namespace ControlHub.Application.Accounts.Interfaces.Repositories
{
    public interface IAccountCommands
    {
        Task AddAsync(Account accDomain, CancellationToken cancellationToken);
        Task UpdateAsync(Account accDomain, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
