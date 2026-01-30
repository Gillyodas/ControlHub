using ControlHub.Application.Users.DTOs;
using ControlHub.SharedKernel.Common;
using ControlHub.Domain.Users;

namespace ControlHub.Application.Users.Interfaces.Repositories
{
    public interface IUserQueries
    {
        Task<User> GetByAccountId(Guid id, CancellationToken cancellationToken);
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PaginatedResult<UserDto>> GetPaginatedAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken);
    }
}
