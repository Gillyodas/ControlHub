using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Domain.Accounts.Identifiers;
using ControlHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.Accounts.Repositories
{
    public class IdentifierConfigRepository : IIdentifierConfigRepository
    {
        private readonly AppDbContext _db;

        public IdentifierConfigRepository(AppDbContext db) => _db = db;

        public async Task<IdentifierConfig?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _db.IdentifierConfigs
                .Include(c => c.Rules)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IdentifierConfig?> GetByNameAsync(string name, CancellationToken ct)
        {
            return await _db.IdentifierConfigs
                .Include(c => c.Rules)
                .FirstOrDefaultAsync(c => c.Name == name, ct);
        }

        public async Task<IEnumerable<IdentifierConfig>> GetActiveConfigsAsync(CancellationToken ct)
        {
            return await _db.IdentifierConfigs
                .Include(c => c.Rules)
                .Where(c => c.IsActive)
                .ToListAsync(ct);
        }

        public async Task AddAsync(IdentifierConfig config, CancellationToken ct)
        {
            await _db.IdentifierConfigs.AddAsync(config, ct);
        }
    }
}
