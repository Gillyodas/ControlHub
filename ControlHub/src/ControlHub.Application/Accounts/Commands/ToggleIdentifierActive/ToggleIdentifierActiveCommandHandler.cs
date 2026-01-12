using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using MediatR;

namespace ControlHub.Application.Accounts.Commands.ToggleIdentifierActive
{
    public class ToggleIdentifierActiveCommandHandler : IRequestHandler<ToggleIdentifierActiveCommand, Result>
    {
        private readonly IIdentifierConfigRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleIdentifierActiveCommandHandler(
            IIdentifierConfigRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ToggleIdentifierActiveCommand request, CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (config == null)
            {
                return Result.Failure(new Error("IdentifierConfig.NotFound", "Identifier configuration not found"));
            }

            if (request.IsActive)
            {
                config.Activate();
            }
            else
            {
                config.Deactivate();
            }

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
