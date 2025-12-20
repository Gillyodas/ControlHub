using ControlHub.Domain.Accounts.Interfaces.Security;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.SharedKernel.Accounts;
using Moq;

namespace ControlHub.Domain.Tests.Accounts.ValueObjects
{
    public class PasswordTests
    {
        private readonly Mock<IPasswordHasher> _hasherMock = new();

        public PasswordTests()
        {
            // Setup mặc định: Hasher trả về hash hợp lệ (để test logic validation của Domain)
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(Password.From(new byte[32], new byte[16])); // 32 bytes hash, 16 bytes salt
        }

        // --- NHÓM 1: BUG HUNTING - ĐỘ MẠNH MẬT KHẨU (COMPLEXITY) ---

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenPasswordIsNuLLOrEmpty(string? input)
        {
            var result = Password.Create(input!, _hasherMock.Object);

            Assert.True(result.IsFailure);
            Assert.Equal(AccountErrors.PasswordIsWeak, result.Error);
        }

        [Theory]
        [InlineData("1234567")]           // < 8 ký tự
        [InlineData("Abcdef1")]           // Thiếu ký tự đặc biệt
        [InlineData("abcdef1@")]          // Thiếu chữ Hoa
        [InlineData("ABCDEF1@")]          // Thiếu chữ thường
        [InlineData("Abcdefgh@")]         // Thiếu số
        public void Create_ShouldFail_WhenPasswordIsWeak(string weakPass)
        {
            // Act
            var result = Password.Create(weakPass, _hasherMock.Object);

            // Assert
            Assert.True(result.IsFailure, $"BUG: Domain chấp nhận mật khẩu yếu: '{weakPass}'");
            Assert.Equal(AccountErrors.PasswordIsWeak, result.Error);
        }

        [Fact]
        public void Create_ShouldSucceed_WhenPasswordIsStrong()
        {
            // Arrange: Đủ 8 ký tự, Hoa, Thường, Số, Đặc biệt
            var strongPass = "StrongP@ss1";

            // Act
            var result = Password.Create(strongPass, _hasherMock.Object);

            // Assert
            Assert.True(result.IsSuccess);
        }

        // --- NHÓM 2: BUG HUNTING - INTEGRITY (TOÀN VẸN DỮ LIỆU HASH) ---

        [Fact]
        public void Create_ShouldFail_WhenHasherReturnsInvalidHashLength()
        {
            // BUG HUNT: Giả sử Hasher bị lỗi, trả về mảng byte rỗng hoặc quá ngắn.
            // Domain Password phải chặn lại để không lưu rác vào DB.

            // Arrange
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(Password.From(new byte[0], new byte[16])); // Hash rỗng (Invalid)

            // Act
            var result = Password.Create("ValidP@ss1", _hasherMock.Object);

            // Assert
            Assert.True(result.IsFailure, "BUG: Domain chấp nhận Hash rỗng (Length=0).");
            Assert.Equal(AccountErrors.PasswordHashFailed, result.Error);
        }

        [Fact]
        public void Create_ShouldFail_WhenHasherReturnsInvalidSaltLength()
        {
            // Arrange
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(Password.From(new byte[32], new byte[5])); // Salt quá ngắn (< 16 bytes)

            // Act
            var result = Password.Create("ValidP@ss1", _hasherMock.Object);

            // Assert
            Assert.True(result.IsFailure, "BUG: Domain chấp nhận Salt quá ngắn.");
            Assert.Equal(AccountErrors.PasswordHashFailed, result.Error);
        }
    }
}