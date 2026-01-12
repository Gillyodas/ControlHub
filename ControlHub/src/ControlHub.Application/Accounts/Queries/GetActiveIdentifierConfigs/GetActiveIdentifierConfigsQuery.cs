using ControlHub.Application.Accounts.DTOs;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Queries.GetActiveIdentifierConfigs
{
    public record GetActiveIdentifierConfigsQuery : IRequest<Result<List<IdentifierConfigDto>>>;
}
