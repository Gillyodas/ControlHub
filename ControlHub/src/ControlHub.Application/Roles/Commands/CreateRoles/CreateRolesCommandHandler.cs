using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Common.Services;
using ControlHub.Domain.Permissions;
using ControlHub.Domain.Roles;
using ControlHub.SharedKernel.Permissions;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Roles;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Roles.Commands.CreateRoles
{
    public class CreateRolesCommandHandler : IRequestHandler<CreateRolesCommand, Result<PartialResult<Role, string>>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleQueries _roleQueries;
        private readonly IPermissionRepository _permissionRepository;
        private readonly CreateRoleWithPermissionsService _createRoleWithPermissionsService;
        private readonly ILogger<CreateRolesCommandHandler> _logger;
        private readonly IUnitOfWork _uow;

        public CreateRolesCommandHandler(
            IRoleRepository roleRepository,
            IRoleQueries roleQueries,
            IPermissionRepository permissionRepository,
            CreateRoleWithPermissionsService createRoleWithPermissionsService,
            ILogger<CreateRolesCommandHandler> logger,
            IUnitOfWork uow)
        {
            _roleRepository = roleRepository;
            _roleQueries = roleQueries;
            _permissionRepository = permissionRepository;
            _createRoleWithPermissionsService = createRoleWithPermissionsService;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Result<PartialResult<Role, string>>> Handle(CreateRolesCommand request, CancellationToken ct)
        {
            _logger.LogInformation("{Code}: {Message}. Count={Count}",
                RoleLogs.CreateRoles_Started.Code,
                RoleLogs.CreateRoles_Started.Message,
                request.Roles?.Count() ?? 0);

            var existingNames = new HashSet<string>(
                (await _roleQueries.GetAllAsync(ct)).Select(r => r.Name.ToLowerInvariant()));

            var validDtos = request.Roles
                .Where(r => !existingNames.Contains(r.Name.ToLowerInvariant()))
                .ToList();

            if (!validDtos.Any())
            {
                _logger.LogWarning("{Code}: {Message}. IncomingCount={Count}",
                    RoleLogs.CreateRoles_NoValidRole.Code,
                    RoleLogs.CreateRoles_NoValidRole.Message,
                    request.Roles?.Count() ?? 0);

                return Result<PartialResult<Role, string>>.Failure(RoleErrors.NoValidRolesCreated);
            }

            var allRequiredPermissionIds = validDtos
                .Where(r => r.PermissionIds != null)
                .SelectMany(r => r.PermissionIds!)
                .Distinct()
                .ToList();

            var allPermissions = await _permissionRepository.GetByIdsAsync(allRequiredPermissionIds, ct);
            var permissionMap = allPermissions.ToDictionary(p => p.Id);

            var successes = new List<Role>();
            var failures = new List<string>();

            foreach (var dto in validDtos)
            {
                if (dto.PermissionIds == null || !dto.PermissionIds.Any())
                {
                    _logger.LogWarning("{Code}: {Message}. Role={RoleName}",
                        RoleLogs.CreateRoles_MissingPermissions.Code,
                        RoleLogs.CreateRoles_MissingPermissions.Message,
                        dto.Name);

                    failures.Add($"{dto.Name}: {RoleErrors.PermissionRequired.Code}");
                    continue;
                }

                var rolePermissions = new List<Permission>();
                var missingPermissions = false;

                foreach (var pId in dto.PermissionIds)
                {
                    if (permissionMap.TryGetValue(pId, out var permissionInstance))
                    {
                        rolePermissions.Add(permissionInstance);
                    }
                    else
                    {
                        missingPermissions = true;
                        break;
                    }
                }

                if (missingPermissions)
                {
                    _logger.LogWarning("{Code}: {Message}. Role={RoleName}",
                        RoleLogs.CreateRoles_NoValidPermissionFound.Code,
                        RoleLogs.CreateRoles_NoValidPermissionFound.Message,
                        dto.Name);

                    failures.Add($"{dto.Name}: {PermissionErrors.PermissionNotFound.Code}");
                    continue;
                }

                var result = _createRoleWithPermissionsService.Handle(dto.Name, dto.Description, rolePermissions);

                if (result.IsSuccess)
                {
                    successes.Add(result.Value);
                    _logger.LogInformation("{Code}: {Message}. Role={RoleName}",
                        RoleLogs.CreateRoles_RolePrepared.Code,
                        RoleLogs.CreateRoles_RolePrepared.Message,
                        dto.Name);
                }
                else
                {
                    failures.Add($"{dto.Name}: {result.Error.Code}");
                    _logger.LogWarning("{Code}: {Message}. Role={RoleName} Error={ErrorCode}",
                        RoleLogs.CreateRoles_RolePrepareFailed.Code,
                        RoleLogs.CreateRoles_RolePrepareFailed.Message,
                        dto.Name,
                        result.Error.Code);
                }
            }

            if (successes.Any())
            {
                await _roleRepository.AddRangeAsync(successes, ct);
                await _uow.CommitAsync(ct);

                _logger.LogInformation("{Code}: {Message}. RolesCreated={Count}",
                    RoleLogs.CreateRoles_Success.Code,
                    RoleLogs.CreateRoles_Success.Message,
                    successes.Count);
            }
            else
            {
                _logger.LogInformation("{Code}: {Message}. No roles persisted.",
                    RoleLogs.CreateRoles_NoPersist.Code,
                    RoleLogs.CreateRoles_NoPersist.Message);
            }

            var partial = PartialResult<Role, string>.Create(successes, failures);

            if (!partial.Successes.Any())
                return Result<PartialResult<Role, string>>.Failure(RoleErrors.NoValidRolesCreated);

            return Result<PartialResult<Role, string>>.Success(partial);
        }
    }
}