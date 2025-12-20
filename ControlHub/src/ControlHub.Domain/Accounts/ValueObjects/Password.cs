using System.Text.RegularExpressions;
using ControlHub.Domain.Accounts.Interfaces.Security;
using ControlHub.Domain.Common;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Results;

namespace ControlHub.Domain.Accounts.ValueObjects
{
    public sealed class Password : ValueObject
    {
        public byte[] Hash { get; private set; }
        public byte[] Salt { get; private set; }

        private Password(byte[] hash, byte[] salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public static Password From(byte[] hash, byte[] salt) => new(hash, salt);

        public static Result<Password> Create(string passStr, IPasswordHasher passwordHasher)
        {
            if (Password.IsWeak(passStr))
            {
                return Result<Password>.Failure(AccountErrors.PasswordIsWeak);
            }
            var pass = passwordHasher.Hash(passStr);

            if (!pass.IsValid())
                return Result<Password>.Failure(AccountErrors.PasswordHashFailed);

            return Result<Password>.Success(pass);
        }

        public static bool IsWeak(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return true;

            if (password.Length < 8) return true;

            if (!Regex.IsMatch(password, @"[a-z]")) return true;

            if (!Regex.IsMatch(password, @"[A-Z]")) return true;

            if (!Regex.IsMatch(password, @"[0-9]")) return true;

            if (!Regex.IsMatch(password, @"[\W_]")) return true;

            return false;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Convert.ToBase64String(Hash);
            yield return Convert.ToBase64String(Salt);
        }

        public bool IsValid()
        {
            // Salt: tối thiểu 16 bytes, tối đa 64 bytes
            // Hash: tối thiểu 32 bytes (SHA-256), tối đa 64 bytes (SHA-512)
            return Hash is { Length: >= 32 and <= 64 }
                && Salt is { Length: >= 16 and <= 64 };
        }
    }
}
