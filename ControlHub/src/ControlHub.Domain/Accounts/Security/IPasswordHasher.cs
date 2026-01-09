using ControlHub.Domain.Accounts.ValueObjects;

namespace ControlHub.Domain.Accounts.Security
{
    public interface IPasswordHasher
    {
        Password Hash(string password);
        bool Verify(string password, Password accPass);
    }
}
