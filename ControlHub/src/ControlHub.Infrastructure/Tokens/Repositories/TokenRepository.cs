using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Tokens;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Tokens.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TokenRepository> _logger;

        public TokenRepository(AppDbContext context, ILogger<TokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Token token, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Tokens.AddAsync(token, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add Token {Value}", token.Value);
                throw new RepositoryException("Error adding token to database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding Token {Value}", token.Value);
                throw new RepositoryException("Unexpected error while adding token.", ex);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during Token SaveChanges");
                throw new RepositoryConcurrencyException("Concurrency conflict while saving tokens.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during Token SaveChanges");
                throw new RepositoryException("Database update error while saving tokens.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Token SaveChanges");
                throw new RepositoryException("Unexpected error during token save operation.", ex);
            }
        }
    }
}