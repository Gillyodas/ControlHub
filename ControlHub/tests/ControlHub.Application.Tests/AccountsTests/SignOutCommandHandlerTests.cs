using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ControlHub.Application.Accounts.Commands.SignOut;
using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Tokens.Interfaces;
using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Tokens;
using ControlHub.Domain.Tokens.Enums;
using ControlHub.SharedKernel.Results;
using ControlHub.SharedKernel.Tokens;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ControlHub.Application.Tests.AccountsTests
{
    public class SignOutCommandHandlerTests
    {
        private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
        private readonly Mock<ITokenQueries> _tokenQueriesMock = new();
        private readonly Mock<ITokenVerifier> _tokenVerifierMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<ILogger<SignOutCommandHandler>> _loggerMock = new();

        private readonly SignOutCommandHandler _handler;

        public SignOutCommandHandlerTests()
        {
            _handler = new SignOutCommandHandler(
                _tokenRepositoryMock.Object,
                _tokenQueriesMock.Object,
                _tokenVerifierMock.Object,
                _uowMock.Object,
                _loggerMock.Object
            );
        }

        // =================================================================================
        // NHÓM 1: SECURITY & VALIDATION (Bảo mật & Xác thực)
        // =================================================================================

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTokenVerificationFails()
        {
            // 🐛 BUG HUNT: Access Token không hợp lệ (hết hạn, sai chữ ký) -> Phải chặn ngay.
            var command = new SignOutCommand("invalid_access_token", "refresh_token");

            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns((ClaimsPrincipal?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(TokenErrors.TokenInvalid, result.Error);
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAccountIdInClaimIsInvalid()
        {
            // 🐛 BUG HUNT: Token giả mạo với Claim "sub" không phải GUID -> Phải chặn.
            var command = new SignOutCommand("access_token", "refresh_token");

            var identity = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid") });
            var principal = new ClaimsPrincipal(identity);

            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns(principal);

            // Mock DB trả về token (để vượt qua check null trước đó)
            var fakeToken = Token.Create(Guid.NewGuid(), "val", TokenType.AccessToken, DateTime.UtcNow.AddMinutes(1));
            _tokenQueriesMock.Setup(q => q.GetByValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(fakeToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(TokenErrors.TokenInvalid, result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAccountIdMismatch()
        {
            // 🐛 BUG HUNT: Hacker dùng Token của mình để logout Token của người khác -> Phải chặn.
            // Token gửi lên (trong DB) thuộc Account A, nhưng Claim trong Token lại là Account B.

            var command = new SignOutCommand("access_token", "refresh_token");
            var tokenOwnerId = Guid.NewGuid();
            var attackerId = Guid.NewGuid();

            var principal = CreatePrincipal(attackerId); // Người gọi là Attacker
            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns(principal);

            // Token trong DB thuộc về Owner
            var accessToken = Token.Create(tokenOwnerId, command.accessToken, TokenType.AccessToken, DateTime.UtcNow.AddMinutes(15));
            var refreshToken = Token.Create(tokenOwnerId, command.refreshToken, TokenType.RefreshToken, DateTime.UtcNow.AddDays(7));

            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(accessToken);
            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.refreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure, "LỖI BẢO MẬT: Cho phép logout token không thuộc về người gọi (Mismatch ID).");
            Assert.Equal(TokenErrors.TokenInvalid, result.Error);
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // =================================================================================
        // NHÓM 2: LOGIC NGHIỆP VỤ (BUSINESS LOGIC)
        // =================================================================================

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTokensNotFoundInStorage()
        {
            // 🐛 BUG HUNT: Token hợp lệ về mặt chữ ký nhưng không có trong DB (đã bị xóa cứng?) -> Coi như lỗi.
            var command = new SignOutCommand("access_token", "refresh_token");
            var principal = CreatePrincipal(Guid.NewGuid());

            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns(principal);

            // DB trả về null
            _tokenQueriesMock.Setup(q => q.GetByValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Token?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(TokenErrors.TokenNotFound, result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTokenAlreadyRevoked()
        {
            // 🐛 BUG HUNT: Cố gắng logout lại một token đã logout rồi.
            // Domain logic của Token.Revoke() sẽ trả về Failure nếu IsRevoked=true.

            var command = new SignOutCommand("access_token", "refresh_token");
            var accountId = Guid.NewGuid();
            var principal = CreatePrincipal(accountId);

            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns(principal);

            // Tạo token đã bị Revoke
            var revokedToken = Token.Rehydrate(Guid.NewGuid(), accountId, command.accessToken, TokenType.AccessToken,
                DateTime.UtcNow.AddMinutes(15), isUsed: false, isRevoked: true, DateTime.UtcNow); // isRevoked = true

            var refreshToken = Token.Create(accountId, command.refreshToken, TokenType.RefreshToken, DateTime.UtcNow.AddDays(7));

            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(revokedToken);
            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.refreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure, "LỖI LOGIC: Không chặn việc logout lại token đã bị thu hồi.");
            Assert.Equal(TokenErrors.TokenAlreadyRevoked, result.Error);
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // =================================================================================
        // NHÓM 3: LUỒNG THÀNH CÔNG (HAPPY PATH)
        // =================================================================================

        [Fact]
        public async Task Handle_ShouldSucceed_AndRevokeBothTokens_WhenAllValid()
        {
            // Arrange
            var command = new SignOutCommand("access_token", "refresh_token");
            var accountId = Guid.NewGuid();
            var principal = CreatePrincipal(accountId);

            _tokenVerifierMock.Setup(v => v.Verify(command.accessToken)).Returns(principal);

            // Tạo token hợp lệ
            var accessToken = Token.Create(accountId, command.accessToken, TokenType.AccessToken, DateTime.UtcNow.AddMinutes(15));
            var refreshToken = Token.Create(accountId, command.refreshToken, TokenType.RefreshToken, DateTime.UtcNow.AddDays(7));

            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(accessToken);
            _tokenQueriesMock.Setup(q => q.GetByValueAsync(command.refreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(refreshToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            // Verify State Change: Cả 2 token phải chuyển sang IsRevoked = true
            Assert.True(accessToken.IsRevoked, "LỖI DATA: Access Token chưa được đánh dấu Revoked.");
            Assert.True(refreshToken.IsRevoked, "LỖI DATA: Refresh Token chưa được đánh dấu Revoked.");

            // Verify Side Effect: Phải Commit xuống DB
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once, "LỖI DATA: Quên gọi Commit để lưu trạng thái.");
        }

        // Helper
        private ClaimsPrincipal CreatePrincipal(Guid accountId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
}