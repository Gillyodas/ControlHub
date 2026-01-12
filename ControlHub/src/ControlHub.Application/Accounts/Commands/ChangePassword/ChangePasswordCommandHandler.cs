using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Accounts.Security;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly ITokenRepository _tokenRepository;

        public ChangePasswordCommandHandler(
            IAccountRepository accountRepository,
            IPasswordHasher passwordHasher,
            ILogger<ChangePasswordCommandHandler> logger,
            IUnitOfWork uow,
            ITokenRepository tokenRepository)
        {
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _uow = uow;
            _tokenRepository = tokenRepository;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}",
                AccountLogs.ChangePassword_Started.Code,
                AccountLogs.ChangePassword_Started.Message,
                request.id);

            var acc = await _accountRepository.GetWithoutUserByIdAsync(request.id, cancellationToken);
            if (acc is null)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_AccountNotFound.Code,
                    AccountLogs.ChangePassword_AccountNotFound.Message,
                    request.id);

                return Result.Failure(AccountErrors.AccountNotFound);
            }

            if (acc.IsDeleted)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_AccountDeleted.Code,
                    AccountLogs.ChangePassword_AccountDeleted.Message,
                    request.id);

                return Result.Failure(AccountErrors.AccountDeleted);
            }

            if (!acc.IsActive)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_AccountDisabled.Code,
                    AccountLogs.ChangePassword_AccountDisabled.Message,
                    request.id);

                return Result.Failure(AccountErrors.AccountDisabled);
            }

            var passIsVerify = _passwordHasher.Verify(request.curPassword, acc.Password);
            if (!passIsVerify)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_InvalidPassword.Code,
                    AccountLogs.ChangePassword_InvalidPassword.Message,
                    request.id);
                return Result.Failure(AccountErrors.InvalidCredentials);
            }

            if (_passwordHasher.Verify(request.newPassword, acc.Password))
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_PasswordSameAsOld.Code,
                    AccountLogs.ChangePassword_PasswordSameAsOld.Message,
                    request.id);
                return Result.Failure(AccountErrors.PasswordSameAsOld);
            }

            var newPass = Password.Create(request.newPassword, _passwordHasher);
            if (newPass.IsFailure)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.ChangePassword_PasswordHashFailed.Code,
                    AccountLogs.ChangePassword_PasswordHashFailed.Message,
                    request.id);
                return Result.Failure(newPass.Error);
            }

            var updateResult = acc.UpdatePassword(newPass.Value);
            if (!updateResult.IsSuccess)
            {
                _logger.LogError("{Code}: {Message} for AccountId {AccountId}. Errors: {Errors}",
                    AccountLogs.ChangePassword_UpdateFailed.Code,
                    AccountLogs.ChangePassword_UpdateFailed.Message,
                    request.id,
                    updateResult.Error);
                return updateResult;
            }

            var tokens = await _tokenRepository.GetTokensByAccountIdAsync(acc.Id, cancellationToken);

            if (tokens.Any())
            {
                foreach (var token in tokens)
                {
                    if (token.IsValid())
                    {
                        token.Revoke();
                    }
                }
            }

            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}",
                AccountLogs.ChangePassword_Success.Code,
                AccountLogs.ChangePassword_Success.Message,
                request.id);

            return Result.Success();
        }
    }
}
