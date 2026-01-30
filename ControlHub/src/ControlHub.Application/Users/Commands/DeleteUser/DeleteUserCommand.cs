using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest<Result<Unit>>;
}
