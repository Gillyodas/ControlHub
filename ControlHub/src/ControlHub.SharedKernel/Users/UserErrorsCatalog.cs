using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Users;

namespace ControlHub.Domain.Users
{
    public class UserErrorsCatalog : IErrorCatalog
    {
        private static readonly Dictionary<string, string> _messageToCode;

        static UserErrorsCatalog()
        {
            _messageToCode = typeof(UserErrors).GetFields()
                .Select(f => f.GetValue(null) as Error)
                .Where(e => e != null)
                .ToDictionary(e => e.Message, e => e.Code);
        }

        public string? GetCodeByMessage(string message)
        {
            return _messageToCode.TryGetValue(message, out var code)
                ? code
                : null; // middleware sẽ fallback sang "Validation.Unknown"
        }
    }
}