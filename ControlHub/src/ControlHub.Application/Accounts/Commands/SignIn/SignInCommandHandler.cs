using ControlHub.Application.Accounts.DTOs;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Tokens.Interfaces;
using ControlHub.Application.Tokens.Interfaces.Generate;
using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Accounts.Identifiers.Services;
using ControlHub.Domain.Accounts.Security;
using ControlHub.Domain.Tokens.Enums;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Tokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControlHub.Application.Accounts.Commands.SignIn
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, Result<SignInDTO>>
    {
        private readonly ILogger<SignInCommandHandler> _logger;
        private readonly IAccountQueries _accountQueries;
        private readonly IdentifierFactory _identifierFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ITokenFactory _tokenFactory;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUnitOfWork _uow;
        private readonly IIdentifierConfigRepository _identifierConfigRepository;

        public SignInCommandHandler(
            ILogger<SignInCommandHandler> logger,
            IAccountQueries accountQueries,
            IdentifierFactory identifierFactory,
            IPasswordHasher passwordHasher,
            IAccessTokenGenerator accessTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenFactory tokenFactory,
            ITokenRepository tokenRepository,
            IUnitOfWork uow,
            IIdentifierConfigRepository identifierConfigRepository)
        {
            _logger = logger;
            _accountQueries = accountQueries;
            _identifierFactory = identifierFactory;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _tokenFactory = tokenFactory;
            _tokenRepository = tokenRepository;
            _uow = uow;
            _identifierConfigRepository = identifierConfigRepository;
        }

        public async Task<Result<SignInDTO>> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{Code}: {Message} for Identifier {Value}",
                AccountLogs.SignIn_Started.Code,
                AccountLogs.SignIn_Started.Message,
                request.Value);

            var varIdentConfig = await _identifierConfigRepository.GetByNameAsync(request.Name, cancellationToken);

            var result = _identifierFactory.Create(request.Type, request.Value);
            if (result.IsFailure)
            {
                _logger.LogWarning("{Code}: {Message} for Identifier {Ident}. Error: {Error}",
                    AccountLogs.SignIn_InvalidIdentifier.Code,
                    AccountLogs.SignIn_InvalidIdentifier.Message,
                    request.Value, result.Error);
                return Result<SignInDTO>.Failure(result.Error);
            }

            var account = await _accountQueries.GetByIdentifierAsync(request.Type, result.Value.NormalizedValue, cancellationToken);
            if (account is null || account.IsDeleted == true || account.IsActive == false)
            {
                _logger.LogWarning("{Code}: {Message} for Identifier {Ident}",
                    AccountLogs.SignIn_AccountNotFound.Code,
                    AccountLogs.SignIn_AccountNotFound.Message,
                    request.Value);
                return Result<SignInDTO>.Failure(AccountErrors.InvalidCredentials);
            }

            var isPasswordValid = _passwordHasher.Verify(request.Password, account.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.SignIn_InvalidPassword.Code,
                    AccountLogs.SignIn_InvalidPassword.Message,
                    account.Id);
                return Result<SignInDTO>.Failure(AccountErrors.InvalidCredentials);
            }

            var roleId = await _accountQueries.GetRoleIdByAccIdAsync(account.Id, cancellationToken);

            if (account.Identifiers == null || !account.Identifiers.Any())
            {
                _logger.LogWarning("{Code}: {Message} for AccountId {AccountId}",
                    AccountLogs.SignIn_InvalidIdentifier.Code,
                    AccountLogs.SignIn_InvalidIdentifier.Message,
                    account.Id);

                return Result<SignInDTO>.Failure(AccountErrors.InvalidCredentials);
            }

            var accessTokenValue = _accessTokenGenerator.Generate(
                account.Id.ToString(),
                account.Identifiers.First().ToString(),
                roleId.ToString());

            var refreshTokenValue = _refreshTokenGenerator.Generate();

            if (string.IsNullOrWhiteSpace(accessTokenValue) || string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                _logger.LogError("{Code}: {Message} during token generation for AccountId {AccountId}",
                    TokenLogs.Refresh_TokenInvalid.Code,
                    "Failed to generate tokens",
                    account.Id);
                return Result<SignInDTO>.Failure(TokenErrors.TokenGenerationFailed);
            }

            var accessToken = _tokenFactory.Create(account.Id, accessTokenValue, TokenType.AccessToken);
            var refreshToken = _tokenFactory.Create(account.Id, refreshTokenValue, TokenType.RefreshToken);

            await _tokenRepository.AddAsync(accessToken, cancellationToken);
            await _tokenRepository.AddAsync(refreshToken, cancellationToken);
            await _uow.CommitAsync(cancellationToken);

            _logger.LogInformation("{Code}: {Message} for AccountId {AccountId}",
                AccountLogs.SignIn_Success.Code,
                AccountLogs.SignIn_Success.Message,
                account.Id);

            var dto = new SignInDTO(
            account.Id,
            account.User?.Username ?? "No name",
            accessTokenValue,
            refreshTokenValue);

            return Result<SignInDTO>.Success(dto);
        }
    }
}