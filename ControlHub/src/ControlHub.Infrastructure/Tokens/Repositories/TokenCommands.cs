using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Tokens;
using ControlHub.Infrastructure.Persistence;
using ControlHub.SharedKernel.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlHub.Infrastructure.Tokens.Repositories
{
    public class TokenCommands : ITokenCommands
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TokenCommands> _logger;

        public TokenCommands(AppDbContext context, ILogger<TokenCommands> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Token token, CancellationToken cancellationToken)
        {
            try
            {
                var entity = TokenMapper.ToEntity(token);
                await _context.Tokens.AddAsync(entity, cancellationToken);
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

        public async Task UpdateAsync(Token token, CancellationToken cancellationToken)
        {
            try
            {
                var entity = TokenMapper.ToEntity(token);
                _context.Tokens.Update(entity);
                await Task.CompletedTask;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while updating Token {Id}", token.Id);
                throw new RepositoryConcurrencyException("Concurrency conflict while updating token.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating Token {Id}", token.Id);
                throw new RepositoryException("Database error while updating token.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating Token {Id}", token.Id);
                throw new RepositoryException("Unexpected error while updating token.", ex);
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