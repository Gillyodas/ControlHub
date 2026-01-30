using ControlHub.Application.Roles.DTOs;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Roles.Queries.GetUserRoles
{
    public record GetUserRolesQuery(Guid UserId) : IRequest<Result<List<RoleDto>>>;
}
