using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Users.Commands.UpdateMyProfile
{
    public sealed record UpdateMyProfileCommand(string? FirstName, string? LastName, string? PhoneNumber) : IRequest<Result>;
}
