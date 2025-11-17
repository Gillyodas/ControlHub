using ControlHub.Application.Accounts.Commands.CreateAccount;
using ControlHub.Application.Accounts.Interfaces;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IAccountValidator _accountValidator;
        private readonly IAccountCommands _accountCommands;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly IAccountFactory _accountFactory;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _uow;

        public RegisterUserCommandHandler(
            IAccountValidator accountValidator,
            IAccountCommands accountCommands,
            ILogger<RegisterUserCommandHandler> logger,
            IAccountFactory accountFactory,
            IConfiguration config,
            IUnitOfWork uow)
        {
            _accountValidator = accountValidator;
            _accountCommands = accountCommands;
            _logger = logger;
            _accountFactory = accountFactory;
            _config = config;
            _uow = uow;
        }

        public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for Ident {Ident}",
                AccountLogs.RegisterUser_Started.Code,
                AccountLogs.RegisterUser_Started.Message,
                request.Value);

            if (await _accountValidator.IdentifierIsExist(request.Value.ToLower(), request.Type, cancellationToken))
            {
                _logger.LogWarning("{Code}: {Message} for Ident {Ident}",
                    AccountLogs.RegisterUser_IdentifierExists.Code,
                    AccountLogs.RegisterUser_IdentifierExists.Message,
                    request.Value);

                return Result<Guid>.Failure(AccountErrors.EmailAlreadyExists);
            }

            var accId = Guid.NewGuid();
            var userRoleId = Guid.Parse(_config["RoleSettings:UserRoleId"]);

            var accountResult = _accountFactory.CreateWithUserAndIdentifier(
                accId,
                request.Value,
                request.Type,
                request.Password,
                userRoleId);

            if (!accountResult.IsSuccess)
            {
                _logger.LogError("{Code}: {Message} for Ident {Ident}. Error: {Error}",
                    AccountLogs.RegisterUser_FactoryFailed.Code,
                    AccountLogs.RegisterUser_FactoryFailed.Message,
                    request.Value,
                    accountResult.Error);

                return Result<Guid>.Failure(accountResult.Error);
            }

            await _accountCommands.AddAsync(accountResult.Value.Value, cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}, Ident {Ident}",
                AccountLogs.RegisterUser_Success.Code,
                AccountLogs.RegisterUser_Success.Message,
                accId,
                request.Value);

            await _uow.CommitAsync(cancellationToken);

            return Result<Guid>.Success(accId);
        }
    }
}
