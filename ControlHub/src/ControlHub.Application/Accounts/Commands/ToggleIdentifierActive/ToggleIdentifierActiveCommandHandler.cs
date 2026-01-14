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
            var configResult = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (configResult.IsFailure)
            {
                return Result.Failure(configResult.Error);
            }

            var config = configResult.Value;

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
