using ControlHub.Application.Accounts.Interfaces;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.RegisterAdmin
{
    public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, Result<Guid>>
    {
        private readonly IAccountValidator _accountValidator;
        private readonly IAccountCommands _accountCommands;
        private readonly ILogger<RegisterAdminCommandHandler> _logger;
        private readonly IAccountFactory _accountFactory;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _uow;

        public RegisterAdminCommandHandler(
            IAccountValidator accountValidator,
            IAccountCommands accountCommands,
            ILogger<RegisterAdminCommandHandler> logger,
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

        public async Task<Result<Guid>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for Ident {Ident}",
                AccountLogs.RegisterAdmin_Started.Code,
                AccountLogs.RegisterAdmin_Started.Message,
                request.Value);

            if (await _accountValidator.IdentifierIsExist(request.Value.ToLower(), request.Type, cancellationToken))
            {
                _logger.LogWarning("{Code}: {Message} for Ident {Ident}",
                    AccountLogs.RegisterAdmin_IdentifierExists.Code,
                    AccountLogs.RegisterAdmin_IdentifierExists.Message,
                    request.Value);

                return Result<Guid>.Failure(AccountErrors.EmailAlreadyExists);
            }

            var accId = Guid.NewGuid();
            var userRoleId = Guid.Parse(_config["RoleSettings:AdminRoleId"]);

            var accountResult = _accountFactory.CreateWithUserAndIdentifier(
                accId,
                request.Value,
                request.Type,
                request.Password,
                userRoleId);

            if (!accountResult.IsSuccess)
            {
                _logger.LogError("{Code}: {Message} for Ident {Ident}. Error: {Error}",
                    AccountLogs.RegisterAdmin_FactoryFailed.Code,
                    AccountLogs.RegisterAdmin_FactoryFailed.Message,
                    request.Value,
                    accountResult.Error);

                return Result<Guid>.Failure(accountResult.Error);
            }

            await _accountCommands.AddAsync(accountResult.Value.Value, cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}, Ident {Ident}",
                AccountLogs.RegisterAdmin_Success.Code,
                AccountLogs.RegisterAdmin_Success.Message,
                accId,
                request.Value);

            await _uow.CommitAsync(cancellationToken);

            return Result<Guid>.Success(accId);
        }
    }
}
