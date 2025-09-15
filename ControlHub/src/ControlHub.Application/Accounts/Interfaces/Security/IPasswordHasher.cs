using ControlHub.SharedKernel.Results;

namespace ControlHub.Application.Accounts.Interfaces.Security
{
    public interface IPasswordHasher
    {
        (byte[] Salt, byte[] Hash) Hash(string password);
        bool Verify(string password, byte[] salt, byte[] expected);
    }
}
