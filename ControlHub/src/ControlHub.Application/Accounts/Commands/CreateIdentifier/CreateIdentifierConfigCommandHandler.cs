using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Domain.Accounts.Identifiers;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.CreateIdentifier
{
    public class CreateIdentifierConfigCommandHandler : IRequestHandler<CreateIdentifierConfigCommand, Result<Guid>>
    {
        private readonly IIdentifierConfigRepository _identifierConfigRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CreateIdentifierConfigCommandHandler> _logger;
        public CreateIdentifierConfigCommandHandler(
            IIdentifierConfigRepository identifierConfigRepository,
            IUnitOfWork uow,
            ILogger<CreateIdentifierConfigCommandHandler> logger)
        {
            _identifierConfigRepository = identifierConfigRepository;
            _uow = uow;
            _logger = logger;
        }
        public async Task<Result<Guid>> Handle(CreateIdentifierConfigCommand request, CancellationToken cancellationToken)
        {
            var existing = await _identifierConfigRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existing != null)
            {
                return Result<Guid>.Failure(Error.Conflict("CONFLICT", "Identifier name already exists"));
            }

            var config = IdentifierConfig.Create(request.Name, request.Description);

            foreach (var ruleDto in request.Rules.OrderBy(r => r.Order))
            {
                var result = config.AddRule(ruleDto.Type, ruleDto.Parameters);

                if (result.IsFailure) return Result<Guid>.Failure(result.Error);
            }

            await _identifierConfigRepository.AddAsync(config, cancellationToken);
            await _uow.CommitAsync(cancellationToken);

            return Result<Guid>.Success(config.Id);
        }
    }
}
