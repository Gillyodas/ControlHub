using ControlHub.Domain.Accounts.Enums;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Commands.RegisterSupperAdmin
{
    public sealed record RegisterSupperAdminCommand(string Value, IdentifierType Type, string Password, string MasterKey) : IRequest<Result<Guid>>;
}
