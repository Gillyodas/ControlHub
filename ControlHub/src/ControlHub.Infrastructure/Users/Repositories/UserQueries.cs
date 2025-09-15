using ControlHub.Application.Users.Interfaces.Repositories;
using ControlHub.Domain.Users;
using ControlHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.Users.Repositories
{
    public class UserQueries : IUserQueries
    {
        private readonly AppDbContext _db;

        public UserQueries(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User> GetByAccountId(Guid id, CancellationToken cancellationToken)
        {
            return UserMapper.ToDomain(await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.AccId == id));
        }
    }
}
