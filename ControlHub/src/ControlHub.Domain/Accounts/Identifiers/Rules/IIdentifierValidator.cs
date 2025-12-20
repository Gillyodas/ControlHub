using ControlHub.Domain.Accounts.Enums;
using ControlHub.SharedKernel.Common.Errors;

namespace ControlHub.Domain.Accounts.Identifiers.Rules
{
    public interface IIdentifierValidator
    {
        IdentifierType Type { get; }
        (bool IsValid, string Normalized, Error? Error) ValidateAndNormalize(string rawValue);
    }
}