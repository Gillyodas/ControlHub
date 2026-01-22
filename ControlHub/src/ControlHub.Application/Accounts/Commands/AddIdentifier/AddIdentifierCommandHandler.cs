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
            _logger.LogInformation("{@LogCode} | AccountId: {AccountId}",
                 AccountLogs.AddIdentifier_Started,
                 request.id);

            // 1. Lấy Aggregate Root (Có Tracking)
            var acc = await _accountRepository.GetWithoutUserByIdAsync(request.id, cancellationToken);

            if (acc is null)
            {
                _logger.LogWarning("{@LogCode} | AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountNotFound,
                    request.id);
                return Result.Failure(AccountErrors.AccountNotFound);
            }

            // 2. Kiểm tra trạng thái Account
            if (acc.IsDeleted)
            {
                _logger.LogWarning("{@LogCode} | AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountDeleted,
                    request.id);
                return Result.Failure(AccountErrors.AccountDeleted);
            }

            if (!acc.IsActive)
            {
                _logger.LogWarning("{@LogCode} | AccountId: {AccountId}",
                    AccountLogs.AddIdentifier_AccountDisabled,
                    request.id);
                return Result.Failure(AccountErrors.AccountDisabled);
            }

            // 3. Tạo Identifier Value Object (Thông qua Domain Service)
            var createIdentResult = _identifierFactory.Create(request.type, request.value);

            if (createIdentResult.IsFailure)
            {
                _logger.LogWarning("{@LogCode} | Error: {Error}",
                    AccountLogs.AddIdentifier_InvalidFormat,
                    createIdentResult.Error.Code);

                return Result.Failure(createIdentResult.Error);
            }

            // 4. Gọi Domain Logic trên Aggregate Root
            var addResult = acc.AddIdentifier(createIdentResult.Value);

            if (addResult.IsFailure)
            {
                _logger.LogWarning("{@LogCode} | Error: {Error}",
                    AccountLogs.AddIdentifier_FailedToAdd,
                    addResult.Error.Code);

                return Result.Failure(addResult.Error);
            }

            // 5. Commit (EF Core tự động Insert vào bảng AccountIdentifiers nhờ OwnsMany)
            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("{@LogCode} | AccountId: {AccountId}",
                 AccountLogs.AddIdentifier_Success,
                 request.id);

            return Result.Success();
        }
    }
}