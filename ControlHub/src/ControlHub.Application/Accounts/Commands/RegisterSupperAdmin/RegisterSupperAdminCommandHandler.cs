using ControlHub.Application.Accounts.Commands.RegisterUser;
using ControlHub.Application.Accounts.Interfaces;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Common.Logs;
using ControlHub.SharedKernel.Constants;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.RegisterSupperAdmin
{
    public class RegisterSupperAdminCommandHandler : IRequestHandler<RegisterSupperAdminCommand, Result<Guid>>
    {
        private readonly IAccountValidator _accountValidator;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly IAccountFactory _accountFactory;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _uow;

        public RegisterSupperAdminCommandHandler(
            IAccountValidator accountValidator,
            IAccountRepository accountRepository,
            ILogger<RegisterUserCommandHandler> logger,
            IAccountFactory accountFactory,
            IConfiguration config,
            IUnitOfWork uow)
        {
            _accountValidator = accountValidator;
            _accountRepository = accountRepository;
            _logger = logger;
            _accountFactory = accountFactory;
            _config = config;
            _uow = uow;
        }

        public async Task<Result<Guid>> Handle(RegisterSupperAdminCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for Ident {Ident}",
                AccountLogs.RegisterSupperAdmin_Started.Code,
                AccountLogs.RegisterSupperAdmin_Started.Message,
                request.Value);

            var masterKey = _config["AppPassword:MasterKey"];

            if (string.IsNullOrEmpty(masterKey))
            {
                _logger.LogError("{Code}: {Message}",
                    CommonLogs.System_ConfigMissing.Code,
                    CommonLogs.System_ConfigMissing.Message);

                return Result<Guid>.Failure(CommonErrors.SystemConfigurationError);
            }

            if (request.MasterKey != masterKey)
            {
                _logger.LogWarning("{Code}: {Message} | Email attempted: {Email}",
                    CommonLogs.Auth_InvalidMasterKey.Code,
                    CommonLogs.Auth_InvalidMasterKey.Message,
                    request.Value);

                return Result<Guid>.Failure(CommonErrors.InvalidMasterKey);
            }

            if (await _accountValidator.IdentifierIsExist(request.Value.ToLower(), request.Type, cancellationToken))
            {
                _logger.LogWarning("{Code}: {Message} for Ident {Ident}",
                    AccountLogs.RegisterSupperAdmin_IdentifierExists.Code,
                    AccountLogs.RegisterSupperAdmin_IdentifierExists.Message,
                    request.Value);

                return Result<Guid>.Failure(AccountErrors.EmailAlreadyExists);
            }

            var accId = Guid.NewGuid();
            var roleIdConfig = _config["RoleSettings:SuperAdminRoleId"];
            Guid superAdminRoleId;
            if (!Guid.TryParse(roleIdConfig, out superAdminRoleId))
            {
                // 2. Nếu Config thiếu hoặc sai -> Fallback về ID mặc định của thư viện
                _logger.LogInformation("RoleSettings:SuperAdminRoleId is missing or invalid. Using Default ID.");
                superAdminRoleId = ControlHubDefaults.Roles.SuperAdminId;
            }

            var accountResult = await _accountFactory.CreateWithUserAndIdentifierAsync(
                accId,
                request.Value,
                request.Type,
                request.Password,
                superAdminRoleId,
                identifierConfigId: request.IdentifierConfigId);

            if (!accountResult.IsSuccess)
            {
                _logger.LogError("{Code}: {Message} for Ident {Ident}. Error: {Error}",
                    AccountLogs.RegisterSupperAdmin_FactoryFailed.Code,
                    AccountLogs.RegisterSupperAdmin_FactoryFailed.Message,
                    request.Value,
                    accountResult.Error);

                return Result<Guid>.Failure(accountResult.Error);
            }

            await _accountRepository.AddAsync(accountResult.Value.Value, cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}, Ident {Ident}",
                AccountLogs.RegisterSupperAdmin_Success.Code,
                AccountLogs.RegisterSupperAdmin_Success.Message,
                accId,
                request.Value);

            await _uow.CommitAsync(cancellationToken);

            return Result<Guid>.Success(accId);
        }
    }
}
