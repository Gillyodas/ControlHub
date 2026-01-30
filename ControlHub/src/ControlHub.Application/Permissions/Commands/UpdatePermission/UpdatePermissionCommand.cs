using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Permissions.Commands.UpdatePermission
{
    public sealed record UpdatePermissionCommand(Guid Id, string Code, string Description) : IRequest<Result>;
}
