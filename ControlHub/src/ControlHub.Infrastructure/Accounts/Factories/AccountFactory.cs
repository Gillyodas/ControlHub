using ControlHub.Application.Accounts.Interfaces;
using ControlHub.Domain.Accounts;
using ControlHub.Domain.Accounts.Enums;
using ControlHub.Domain.Accounts.Identifiers.Interfaces;
using ControlHub.Domain.Accounts.Interfaces.Security;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.Domain.Users;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Infrastructure.Accounts.Factories
{
    public class AccountFactory : IAccountFactory
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IIdentifierValidatorFactory _identifierValidatorFactory;

        public AccountFactory(
            IPasswordHasher passwordHasher,
            IIdentifierValidatorFactory identifierValidatorFactory)
        {
            _passwordHasher = passwordHasher;
            _identifierValidatorFactory = identifierValidatorFactory;
        }

        public Result<Maybe<Account>> CreateWithUserAndIdentifier(
            Guid accountId,
            string identifierValue,
            IdentifierType identifierType,
            string rawPassword,
            Guid roleId,
            string? username = "No name")
        {
            var pass = _passwordHasher.Hash(rawPassword);

            var account = Account.Create(accountId, pass, roleId);

            var validator = _identifierValidatorFactory.Get(identifierType);
            if (validator == null)
                return Result<Maybe<Account>>.Failure(AccountErrors.UnsupportedIdentifierType);

            var (isValid, normalized, error) = validator.ValidateAndNormalize(identifierValue);

            if (!isValid)
                return Result<Maybe<Account>>.Failure(error);

            var ident = Identifier.Create(identifierType, identifierValue, normalized);

            var addResult = account.AddIdentifier(ident);
            if (!addResult.IsSuccess)
                return Result<Maybe<Account>>.Failure(addResult.Error);

            var user = new User(Guid.NewGuid(), accountId, username);

            var attachResult = account.AttachUser(user);
            if (!attachResult.IsSuccess)
                return Result<Maybe<Account>>.Failure(attachResult.Error);

            return Result<Maybe<Account>>.Success(Maybe<Account>.From(account));
        }
    }
}
