using ControlHub.Domain.Tokens;

namespace ControlHub.Application.Tokens.Interfaces.Repositories
{
    public interface ITokenCommands
    {
        Task AddAsync(Token token, CancellationToken cancellationToken);
        Task UpdateAsync(Token token, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
