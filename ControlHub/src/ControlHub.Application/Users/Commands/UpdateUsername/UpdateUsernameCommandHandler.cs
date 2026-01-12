using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Users.Interfaces.Repositories;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Users.Commands.UpdateUsername
{
    public class UpdateUsernameCommandHandler : IRequestHandler<UpdateUsernameCommand, Result<string>>
    {
        private readonly ILogger<UpdateUsernameCommandHandler> _logger;
        private readonly IUnitOfWork _uow;
        private IUserRepository _userRepository;

        public UpdateUsernameCommandHandler(ILogger<UpdateUsernameCommandHandler> logger, IUserRepository userRepo, IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
            _userRepository = userRepo;
        }

        public async Task<Result<string>> Handle(UpdateUsernameCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAccountId(request.id, cancellationToken);

            if (user is null)
                return Result<string>.Failure(UserErrors.NotFound);

            var updateResult = user.UpdateUsername(request.username);

            if (!updateResult.IsSuccess)
                return Result<string>.Failure(updateResult.Error);
            

            await _uow.CommitAsync(cancellationToken);

            return Result<string>.Success(user.Username);
        }
    }
}
