using MediatR;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Application.Users.Commands.UpdateUsername
{
    public record UpdateUsernameCommand(Guid id, string username) : IRequest<Result<string>>;
}
