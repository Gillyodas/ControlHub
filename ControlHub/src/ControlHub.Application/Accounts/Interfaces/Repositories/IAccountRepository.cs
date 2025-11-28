using ControlHub.Domain.Accounts;
namespace ControlHub.Application.Accounts.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task AddAsync(Account acc, CancellationToken cancellationToken);
        Task<Account?> GetWithoutUserByIdAsync(
            Guid id,
            CancellationToken cancellationToken);
    }
}
