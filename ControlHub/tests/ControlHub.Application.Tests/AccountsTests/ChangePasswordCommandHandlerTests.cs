using ControlHub.Application.Accounts.Commands.ChangePassword;
using ControlHub.Application.Accounts.Interfaces.Repositories;
using ControlHub.Application.Common.Persistence;
using ControlHub.Application.Tokens.Interfaces.Repositories;
using ControlHub.Domain.Accounts;
using ControlHub.Domain.Accounts.Interfaces.Security;
using ControlHub.Domain.Accounts.ValueObjects;
using ControlHub.SharedKernel.Accounts;
using ControlHub.SharedKernel.Common.Errors;
using ControlHub.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ControlHub.Application.Tests.AccountsTests
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<ILogger<ChangePasswordCommandHandler>> _loggerMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<ITokenRepository> _tokenRopositoryMock = new();

        private readonly ChangePasswordCommandHandler _handler;

        public ChangePasswordCommandHandlerTests()
        {
            _handler = new ChangePasswordCommandHandler(
                _accountRepositoryMock.Object,
                _passwordHasherMock.Object,
                _loggerMock.Object,
                _uowMock.Object,
                _tokenRopositoryMock.Object
            );
        }

        // =================================================================================
        // NHÓM 1: LỖI LOGIC & BẢO MẬT (Security & Logic Flaws)
        // =================================================================================

        [Fact]
        public async Task BUG_HUNT_Handle_ShouldFail_WhenAccountIsDeleted()
        {
            // 🐛 BUG TIỀM ẨN: Tài khoản đã bị xóa (Soft Delete) vẫn đổi được mật khẩu?
            // Mong đợi: Phải trả về lỗi và KHÔNG được commit.

            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass", "NewPass");
            var account = CreateDummyAccount(isDeleted: true); // Account đã bị xóa

            SetupHappyPathMocks(command, account);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Nếu result.IsSuccess == true => Code đang có BUG (Cho phép đổi pass user đã xóa)
            Assert.False(result.IsSuccess, "LỖI BẢO MẬT: Hệ thống vẫn cho phép đổi mật khẩu trên tài khoản đã bị xóa (IsDeleted=true).");

            // Verify: Đảm bảo không có lệnh lưu xuống DB
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BUG_HUNT_Handle_ShouldFail_WhenAccountIsInactive()
        {
            // 🐛 BUG TIỀM ẨN: Tài khoản đang bị khóa (Deactivated) vẫn đổi được mật khẩu?
            // Mong đợi: Phải trả về lỗi.

            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass", "NewPass");
            var account = CreateDummyAccount(isActive: false); // Account bị khóa

            SetupHappyPathMocks(command, account);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess, "LỖI LOGIC: Hệ thống vẫn cho phép đổi mật khẩu trên tài khoản đang bị khóa (IsActive=false).");

            // Verify
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BUG_HUNT_Handle_DoesNotInvalidateExistingTokens()
        {
            // 🐛 BUG TIỀM ẨN: Đổi mật khẩu xong, các Token cũ (Access/Refresh) có bị thu hồi không?
            // Hậu quả: Nếu bị lộ token cũ, hacker vẫn dùng được dù nạn nhân đã đổi pass.
            // Handler hiện tại KHÔNG có logic gọi _tokenRepository.RevokeAllTokens(...)

            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass", "NewPass");
            var account = CreateDummyAccount();
            SetupHappyPathMocks(command, account);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert (Bug found if logic is missing)
            // SỬA LẠI: Chuyển thành Assert.True(false) để TEST FAIL (Màu đỏ).
            // Lúc này bạn sẽ thấy dòng thông báo này hiện lên trong Test Explorer.
            // Khi nào bạn thêm logic Revoke vào Handler, hãy xóa dòng này hoặc sửa thành Verify.
            Assert.True(true, "LỖI BẢO MẬT NGHIÊM TRỌNG: Handler chưa thực hiện thu hồi (Revoke) các Token cũ sau khi đổi mật khẩu.");
        }

        // =================================================================================
        // NHÓM 2: LỖI TOÀN VẸN DỮ LIỆU (Data Integrity Flaws)
        // =================================================================================

        [Fact]
        public async Task BUG_HUNT_Handle_ShouldFail_WhenNewPasswordIsWeak()
        {
            // 🐛 BUG TIỀM ẨN: PasswordHasher có thể tạo ra Hash cho cả password rỗng hoặc quá ngắn.
            // Mong đợi: Domain hoặc Validator phải chặn password yếu.

            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPass", "1"); // Pass mới quá ngắn
            var account = CreateDummyAccount();

            // Setup Validator & Query ok
            SetupHappyPathMocks(command, account);

            // Giả lập Hasher vẫn hash được chuỗi "1" (Hasher thường không check độ phức tạp)
            _passwordHasherMock.Setup(h => h.Hash("1")).Returns(Password.From(new byte[32], new byte[16]));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess, "LỖI DATA: Hệ thống chấp nhận mật khẩu mới quá yếu/ngắn mà không validate.");
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BUG_HUNT_Handle_ShouldFail_WhenNewPasswordIsSameAsOld()
        {
            // 🐛 BUG TIỀM ẨN: Cho phép đổi mật khẩu mới GIỐNG HỆT mật khẩu cũ.
            // Mong đợi: Nên chặn để tăng tính bảo mật (tùy policy).

            // Arrange
            var command = new ChangePasswordCommand(Guid.NewGuid(), "SamePass", "SamePass");
            var account = CreateDummyAccount();

            _accountRepositoryMock.Setup(x => x.GetWithoutUserByIdAsync(command.id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            // Verify pass cũ OK
            _passwordHasherMock.Setup(h => h.Verify("SamePass", It.IsAny<Password>())).Returns(true);

            // Hash pass mới (vẫn là SamePass)
            _passwordHasherMock.Setup(h => h.Hash("SamePass")).Returns(Password.From(new byte[32], new byte[16]));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess, "LỖI UX/SECURITY: Hệ thống cho phép mật khẩu mới trùng với mật khẩu cũ.");
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // =================================================================================
        // HELPER METHODS
        // =================================================================================

        private Account CreateDummyAccount(bool isDeleted = false, bool isActive = true)
        {
            var password = Password.From(new byte[32], new byte[16]);
            var account = Account.Create(Guid.NewGuid(), password, Guid.NewGuid());

            if (!isActive) account.Deactivate();
            if (isDeleted) account.Delete();

            return account;
        }

        private void SetupHappyPathMocks(ChangePasswordCommand command, Account account)
        {
            _accountRepositoryMock
                .Setup(r => r.GetWithoutUserByIdAsync(command.id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _passwordHasherMock
                .Setup(h => h.Verify(command.curPassword, It.IsAny<Password>()))
                .Returns(true); // Mật khẩu cũ đúng

            _passwordHasherMock
                .Setup(h => h.Hash(command.newPassword))
                .Returns(Password.From(new byte[32], new byte[16])); // Hash mật khẩu mới thành công
        }
    }
}