using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Domain.Accounts.Identifiers.Services;
using ControlHub.SharedKernel.Accounts; // Chứa AccountLogs, AccountErrors
using ControlHub.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.AddIdentifier
{
    public class AddIdentifierCommandHandler : IRequestHandler<AddIdentifierCommand, Result>
    {
        private readonly ILogger<AddIdentifierCommandHandler> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IAccountRepository _accountRepository;
        private readonly IdentifierFactory _identifierFactory;

        public AddIdentifierCommandHandler(
            ILogger<AddIdentifierCommandHandler> logger,
            IUnitOfWork uow,
            IAccountRepository accountRepository,
            IdentifierFactory identifierFactory)
        {
            _logger = logger;
            _uow = uow;
            _accountRepository = accountRepository;
            _identifierFactory = identifierFactory;
        }

        public async Task<Result> Handle(AddIdentifierCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for AccountId: {AccountId}",
                 AccountLogs.AddIdentifier_Started.Code,
                 AccountLogs.AddIdentifier_Started.Message,
                 request.id);

            // 1. Lấy Aggregate Root (Có Tracking)
            var acc = await _accountRepository.GetWithoutUserByIdAsync(request.id, cancellationToken);

            if (acc is null)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountNotFound.Code,
                    AccountLogs.AddIdentifier_AccountNotFound.Message,
                    request.id);
                return Result.Failure(AccountErrors.AccountNotFound);
            }

            // 2. Kiểm tra trạng thái Account
            if (acc.IsDeleted)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountDeleted.Code,
                    AccountLogs.AddIdentifier_AccountDeleted.Message,
                    request.id);
                return Result.Failure(AccountErrors.AccountDeleted);
            }

            if (!acc.IsActive)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountDisabled.Code,
                    AccountLogs.AddIdentifier_AccountDisabled.Message,
                    request.id);
                return Result.Failure(AccountErrors.AccountDisabled);
            }

            // 3. Tạo Identifier Value Object (Thông qua Domain Service)
            var createIdentResult = _identifierFactory.Create(request.type, request.value);

            if (createIdentResult.IsFailure)
            {
                _logger.LogWarning("{Code}: {Message} | Error: {Error}",
                    AccountLogs.AddIdentifier_InvalidFormat.Code,
                    AccountLogs.AddIdentifier_InvalidFormat.Message,
                    createIdentResult.Error.Code);

                return Result.Failure(createIdentResult.Error);
            }

            // 4. Gọi Domain Logic trên Aggregate Root
            var addResult = acc.AddIdentifier(createIdentResult.Value);

            if (addResult.IsFailure)
            {
                _logger.LogWarning("{Code}: {Message} | Error: {Error}",
                    AccountLogs.AddIdentifier_FailedToAdd.Code,
                    AccountLogs.AddIdentifier_FailedToAdd.Message,
                    addResult.Error.Code);

                return Result.Failure(addResult.Error);
            }

            // 5. Commit (EF Core tự động Insert vào bảng AccountIdentifiers nhờ OwnsMany)
            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId: {AccountId}",
                 AccountLogs.AddIdentifier_Success.Code,
                 AccountLogs.AddIdentifier_Success.Message,
                 request.id);

            return Result.Success();
        }
    }
}