using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlHub.Application.Accounts.DTOs;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Queries.GetIdentifierConfigs
{
    public class GetIdentifierConfigsQueryHandler : IRequestHandler<GetIdentifierConfigsQuery, Result<List<IdentifierConfigDto>>>
    {
        private readonly IIdentifierConfigRepository _repo;

        public GetIdentifierConfigsQueryHandler(IIdentifierConfigRepository repo)
        {
            _repo = repo;
        }

        public async Task<Result<List<IdentifierConfigDto>>> Handle(
            GetIdentifierConfigsQuery request,
            CancellationToken ct)
        {
            var configsResult = await _repo.GetActiveConfigsAsync(ct);
            if (configsResult.IsFailure)
            {
                return Result<List<IdentifierConfigDto>>.Failure(configsResult.Error);
            }

            var dtos = configsResult.Value.Select(c => new IdentifierConfigDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.Rules.Select(r => new ValidationRuleDto(
                    r.Type,
                    r.GetParameters(),
                    r.ErrorMessage,
                    r.Order
                )).ToList()
            )).ToList();

            return Result<List<IdentifierConfigDto>>.Success(dtos);
        }
    }
}
