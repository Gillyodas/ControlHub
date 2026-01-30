using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Permissions.Interfaces.Repositories;
using ControlHub.SharedKernel.Permissions;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Permissions.Commands.DeletePermission
{
    public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<DeletePermissionCommandHandler> _logger;

        public DeletePermissionCommandHandler(
            IPermissionRepository permissionRepository,
            IUnitOfWork uow,
            ILogger<DeletePermissionCommandHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken);

            if (permission == null)
            {
                _logger.LogWarning("Permission with ID {PermissionId} not found for deletion.", request.Id);
                return Result.Failure(PermissionErrors.PermissionNotFound);
            }

            await _permissionRepository.DeleteAsync(permission, cancellationToken);
            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("Permission {PermissionId} deleted successfully.", request.Id);

            return Result.Success();
        }
    }
}
