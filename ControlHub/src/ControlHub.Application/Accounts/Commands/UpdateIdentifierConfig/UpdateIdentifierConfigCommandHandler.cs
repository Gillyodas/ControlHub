using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Domain.Accounts.Identifiers;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Commands.UpdateIdentifierConfig
{
    public class UpdateIdentifierConfigCommandHandler : IRequestHandler<UpdateIdentifierConfigCommand, Result>
    {
        private readonly IIdentifierConfigRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateIdentifierConfigCommandHandler(
            IIdentifierConfigRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateIdentifierConfigCommand request, CancellationToken cancellationToken)
        {
            var configResult = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (configResult.IsFailure)
            {
                return Result.Failure(configResult.Error);
            }

            var config = configResult.Value;

            // Check if name is being changed and if new name already exists
            if (config.Name != request.Name)
            {
                var existingResult = await _repository.GetByNameAsync(request.Name, cancellationToken);
                if (existingResult.IsSuccess && existingResult.Value.Id != config.Id)
                {
                    return Result.Failure(new Error("IdentifierConfig.DuplicateName", "An identifier configuration with this name already exists"));
                }
            }

            // Update basic properties
            config.UpdateName(request.Name);
            config.UpdateDescription(request.Description);

            // Update rules
            var validationRules = request.Rules.Select(r => 
                ValidationRule.Create(r.Type, r.Parameters, r.ErrorMessage, r.Order).Value
            ).ToList();

            config.UpdateRules(validationRules);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
