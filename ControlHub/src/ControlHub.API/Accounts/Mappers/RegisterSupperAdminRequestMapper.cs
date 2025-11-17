using ControlHub.API.Accounts.ViewModels.Request;
using ControlHub.Application.Accounts.Commands.RegisterSupperAdmin;
using ControlHub.Domain.Accounts.Enums;

namespace ControlHub.API.Accounts.Mappers
{
    public class RegisterSupperAdminRequestMapper
    {
        public static RegisterSupperAdminCommand ToCommand(RegisterSupperAdminRequest request)
        {
            if (!Enum.TryParse<IdentifierType>(request.Type, ignoreCase: true, out var type))
                throw new ArgumentException("Unsupported identifier type");

            return new RegisterSupperAdminCommand(request.Value, type, request.Password, request.MasterKey);
        }
    }
}
