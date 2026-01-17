using ControlHub.Domain.Accounts;
using ControlHub.Domain.Accounts.Enums;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Application.Accounts.Interfaces
{
    public interface IAccountFactory
    {
        Task<Result<Maybe<Account>>> CreateWithUserAndIdentifierAsync(
            Guid accountId,
            string identifierValue,
            IdentifierType identifierType,
            string rawPassword,
            Guid roleId,
            string? username = "No name",
            Guid? identifierConfigId = null);
    }
}
