using ControlHub.Domain.Tokens;

namespace ControlHub.Application.Tokens.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        Task AddAsync(Token token, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
