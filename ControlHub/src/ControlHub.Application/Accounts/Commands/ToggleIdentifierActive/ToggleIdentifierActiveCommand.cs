using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Commands.ToggleIdentifierActive
{
    public record ToggleIdentifierActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
}
