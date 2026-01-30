using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Permissions.Commands.DeletePermission
{
    public sealed record DeletePermissionCommand(Guid Id) : IRequest<Result>;
}
