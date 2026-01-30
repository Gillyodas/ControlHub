using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.SharedKernel.Permissions;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Permissions.Commands.UpdatePermission
{
    public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UpdatePermissionCommandHandler> _logger;

        public UpdatePermissionCommandHandler(
            IPermissionRepository permissionRepository,
            IUnitOfWork uow,
            ILogger<UpdatePermissionCommandHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken);

            if (permission == null)
            {
                _logger.LogWarning("Permission with ID {PermissionId} not found for update.", request.Id);
                return Result.Failure(PermissionErrors.PermissionNotFound);
            }

            var updateResult = permission.Update(request.Code, request.Description);

            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("Permission {PermissionId} updated successfully to {Code}.", request.Id, request.Code);

            return Result.Success();
        }
    }
}
