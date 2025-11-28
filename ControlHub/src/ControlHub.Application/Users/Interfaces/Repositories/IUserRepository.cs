using ControlHub.Domain.Users;

namespace ControlHub.Application.Users.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
