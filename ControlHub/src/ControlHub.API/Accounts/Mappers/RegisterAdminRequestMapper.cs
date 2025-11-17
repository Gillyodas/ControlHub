using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.Application.Accounts.Commands.RegisterAdmin;
using ControlHub.Domain.Accounts.Enums;

namespace ControlHub.API.Accounts.Mappers
{
    public class RegisterAdminRequestMapper
    {
        public static RegisterAdminCommand ToCommand(RegisterAdminRequest request)
        {
            if (!Enum.TryParse<IdentifierType>(request.Type, ignoreCase: true, out var type))
                throw new ArgumentException("Unsupported identifier type");

            return new RegisterAdminCommand(request.Value, type, request.Password);
        }
    }
}
