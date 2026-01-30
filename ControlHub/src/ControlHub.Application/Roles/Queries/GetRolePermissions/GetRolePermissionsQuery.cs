using ControlHub.Application.Permissions.DTOs;
using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Roles;
using MediatR;

namespace ControlHub.Application.Roles.Queries.GetRolePermissions
{
    public sealed record GetRolePermissionsQuery(Guid RoleId) : IRequest<Result<List<PermissionDto>>>;

    public sealed class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<List<PermissionDto>>>
    {
        private readonly IRoleRepository _roleRepository;

        public GetRolePermissionsQueryHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Result<List<PermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

            if (role == null)
            {
                return Result<List<PermissionDto>>.Failure(RoleErrors.RoleNotFound);
            }

            var permissions = role.Permissions.Select(p => new PermissionDto(p.Id, p.Code, p.Description)).ToList();

            return Result<List<PermissionDto>>.Success(permissions);
        }
    }
}
