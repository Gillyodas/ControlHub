using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Permissions;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Roles;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Roles.Commands.SetRolePermissions
{
    public class AddPermissonsForRoleCommandHandler : IRequestHandler<AddPermissonsForRoleCommand, Result<PartialResult<Permission, string>>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionQueries _permissionQueries;
        private readonly ILogger<AddPermissonsForRoleCommandHandler> _logger;
        private readonly IUnitOfWork _uow;

        public AddPermissonsForRoleCommandHandler(
            IRoleRepository roleRepository,
            IPermissionQueries permissionQueries,
            ILogger<AddPermissonsForRoleCommandHandler> logger,
            IUnitOfWork uow)
        {
            _roleRepository = roleRepository;
            _permissionQueries = permissionQueries;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Result<PartialResult<Permission, string>>> Handle(AddPermissonsForRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "{@LogCode} | RoleId: {RoleId}",
                RoleLogs.SetPermissions_Started,
                request.roleId);

            if (!Guid.TryParse(request.roleId, out var roleId))
            {
                _logger.LogWarning(
                    "{@LogCode} | Invalid format: {RoleId}",
                    RoleLogs.SetPermissions_InvalidRoleId,
                    request.roleId);
                return Result<PartialResult<Permission, string>>.Failure(RoleErrors.InvalidRoleIdFormat);
            }

            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                _logger.LogWarning(
                    "{@LogCode} | RoleId: {RoleId}",
                    RoleLogs.SetPermissions_RoleNotFound,
                    roleId);
                return Result<PartialResult<Permission, string>>.Failure(RoleErrors.RoleNotFound);
            }

            var requestedPermissionGuids = new HashSet<Guid>();
            var nonGuidFailures = new List<string>();

            foreach (var permissionIdString in request.permissionIds)
            {
                if (Guid.TryParse(permissionIdString, out var perId))
                    requestedPermissionGuids.Add(perId);
                else
                    nonGuidFailures.Add($"Invalid GUID format: '{permissionIdString}'");
            }

            var existingPermissions = await _permissionQueries.GetByIdsAsync(requestedPermissionGuids, cancellationToken);

            var domainPartialResult = role.AddRangePermission(existingPermissions);

            var allFailures = domainPartialResult.Value.Failures.ToList();

            allFailures.AddRange(nonGuidFailures);

            var foundPermissionIds = existingPermissions.Select(p => p.Id).ToHashSet();
            var nonExistentIds = requestedPermissionGuids
                .Where(id => !foundPermissionIds.Contains(id))
                .Select(id => $"PermissionId not found: '{id}'");
            allFailures.AddRange(nonExistentIds);

            var finalPartialResult = PartialResult<Permission, string>.Create(
                domainPartialResult.Value.Successes,
                allFailures);

            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "{@LogCode} | Role: {RoleName} | Success: {SuccessCount} | Failure: {FailureCount}",
                RoleLogs.SetPermissions_Finished,
                role.Name,
                finalPartialResult.Successes.Count,
                finalPartialResult.Failures.Count);

            return Result<PartialResult<Permission, string>>.Success(finalPartialResult);
        }
    }
}