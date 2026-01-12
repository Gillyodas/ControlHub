using ControlHub.Application.Accounts.DTOs;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Commands.UpdateIdentifierConfig
{
    public record UpdateIdentifierConfigCommand(
        Guid Id,
        string Name,
        string Description,
        List<ValidationRuleDto> Rules
    ) : IRequest<Result>;
}
