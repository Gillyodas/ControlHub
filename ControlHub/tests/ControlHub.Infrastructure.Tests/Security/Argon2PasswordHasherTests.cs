using ControlHub.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace ControlHub.Infrastructure.Tests.Security
{
    public class Argon2PasswordHasherTests
    {
        private readonly Argon2PasswordHasher _hasher;

        public Argon2PasswordHasherTests()
        {
            // Setup Argon2Options cho test (thông số nhỏ để chạy nhanh hơn)
            var options = Options.Create(new Argon2Options
            {
                SaltSize = 16,
                HashSize = 16,
                MemorySizeKB = 32 * 1024,   // 32MB RAM
                Iterations = 2,
                DegreeOfParallelism = 2
            });

            _hasher = new Argon2PasswordHasher(options);
        }

        [Fact]
        public void Hash_ShouldReturn_ValidPHCString()
        {
            // Arrange
            var password = "Secret123!";

            // Act
            var hash = _hasher.Hash(password);

            // Assert
            Assert.StartsWith("$argon2id$v=19$m=", hash); // kiểm tra format PHC
            Assert.Contains('$', hash); // ít nhất có dấu phân tách
        }

        [Fact]
        public void Verify_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            var password = "Secret123!";
            var hash = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForWrongPassword()
        {
            // Arrange
            var hash = _hasher.Hash("Secret123!");

            // Act
            var result = _hasher.Verify("WrongPass!", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Hash_ShouldThrow_WhenPasswordIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _hasher.Hash(null!));
        }

        [Fact]
        public void Hash_ShouldThrow_WhenPasswordIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => _hasher.Hash(string.Empty));
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenPHCStringIsInvalidFormat()
        {
            // PHC sai format → nên trả về false
            var result = _hasher.Verify("pass", "not-a-phc-string");
            Assert.False(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenSaltIsCorrupted()
        {
            // Arrange
            var password = "Secret123!";
            var hash = _hasher.Hash(password);

            // corrupt salt (phần giữa)
            var parts = hash.Split('$');
            parts[3] = "!!!INVALIDBASE64!!!";
            var corrupted = string.Join("$", parts);

            // Act
            var result = _hasher.Verify(password, corrupted);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenHashIsCorrupted()
        {
            var password = "Secret123!";
            var hash = _hasher.Hash(password);

            // corrupt hash (phần cuối)
            var parts = hash.Split('$');
            parts[4] = "ZmFrZWhhc2g="; // base64 của "fakehash"
            var corrupted = string.Join("$", parts);

            var result = _hasher.Verify(password, corrupted);

            Assert.False(result);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenOptionsInvalid()
        {
            // memory = 0 → không hợp lệ
            var invalidOpt = Options.Create(new Argon2Options
            {
                SaltSize = 16,
                HashSize = 16,
                MemorySizeKB = 0,
                Iterations = 2,
                DegreeOfParallelism = 2
            });

            Assert.Throws<ArgumentException>(() => new Argon2PasswordHasher(invalidOpt));
        }
    }
}
