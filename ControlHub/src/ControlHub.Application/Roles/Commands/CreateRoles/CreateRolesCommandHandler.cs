using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Roles;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Roles;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Roles.Commands.CreateRoles
{
    public class CreateRolesCommandHandler : IRequestHandler<CreateRolesCommand, Result>
    {
        private readonly IRoleCommands _roleCommands;
        private readonly IRoleQueries _roleQueries;
        private readonly ILogger<CreateRolesCommandHandler> _logger;
        private readonly IUnitOfWork _uow;

        public CreateRolesCommandHandler(
            IRoleCommands roleCommands,
            IRoleQueries roleQueries,
            ILogger<CreateRolesCommandHandler> logger,
            IUnitOfWork uow)
        {
            _roleCommands = roleCommands;
            _roleQueries = roleQueries;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Result> Handle(CreateRolesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message}. Count={Count}",
                RoleLogs.CreateRoles_Started.Code,
                RoleLogs.CreateRoles_Started.Message,
                request.Roles.Count());

            try
            {
                // Kiểm tra trùng tên role
                var existingRoles = await _roleQueries.GetAllAsync(cancellationToken);
                var duplicates = request.Roles
                    .Where(r => existingRoles.Any(e => e.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (duplicates.Any())
                {
                    _logger.LogWarning("{Code}: {Message}. Duplicates={Names}",
                        RoleLogs.CreateRoles_DuplicateNames.Code,
                        RoleLogs.CreateRoles_DuplicateNames.Message,
                        string.Join(", ", duplicates.Select(d => d.Name)));

                    return Result.Failure(RoleErrors.RoleNameAlreadyExists);
                }

                var roles = request.Roles
                    .Select(r => Role.Create(Guid.NewGuid(), r.Name, r.Description))
                    .ToList();

                await _roleCommands.AddRangeAsync(roles, cancellationToken);
                await _uow.CommitAsync(cancellationToken);

                _logger.LogInformation("{Code}: {Message}. Created={Count}",
                    RoleLogs.CreateRoles_Success.Code,
                    RoleLogs.CreateRoles_Success.Message,
                    roles.Count);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Code}: {Message}",
                    RoleLogs.CreateRoles_Failed.Code,
                    RoleLogs.CreateRoles_Failed.Message);

                return Result.Failure(RoleErrors.RoleUnexpectedError);
            }
        }
    }
}