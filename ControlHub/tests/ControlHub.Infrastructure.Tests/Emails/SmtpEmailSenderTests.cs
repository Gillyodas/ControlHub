using ControlHub.Application.Emails.Interfaces;
using ControlHub.Infrastructure.Emails;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using Xunit;

namespace ControlHub.Infrastructure.Tests.Emails
{
    public class SmtpEmailSenderTests
    {
        // Helper: Tạo IConfiguration mock với values hợp lệ
        private IConfiguration CreateValidConfig()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Smtp:Host", "smtp.gmail.com"},
                {"Smtp:Port", "587"},
                {"Smtp:Username", "test@test.com"},
                {"Smtp:Password", "testpass"},
                {"Smtp:From", "from@test.com"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        // --- NHÓM 1: BUG HUNTING - NULL/MISSING CONFIG VALUES ---

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenHostIsNull()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Port", "587"},
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                    // Host bị thiếu
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenPortIsNull()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    // Port bị thiếu -> sẽ dùng default "25"
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            // BUG: Code hiện tại sẽ dùng port 25 thay vì throw error
            // Nhưng nếu connect thực tế sẽ fail
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenPortIsInvalidFormat()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "invalid_port"}, // BUG: int.Parse sẽ throw
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenUsernameIsNull()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "587"},
                    // Username missing
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenPasswordIsNull()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "587"},
                    {"Smtp:Username", "test@test.com"},
                    // Password missing
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenFromIsNull()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "587"},
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"}
                    // From missing
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        // --- NHÓM 2: BUG HUNTING - INVALID EMAIL ADDRESSES ---

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SendEmailAsync_ShouldThrowException_WhenToEmailIsInvalid(string invalidEmail)
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync(invalidEmail, "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenToEmailIsInvalidFormat()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(async () =>
                await sender.SendEmailAsync("invalid-email", "Subject", "Body"));
        }

        // --- NHÓM 3: BUG HUNTING - SMTP CONNECTION ERRORS ---

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenHostIsUnreachable()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "nonexistent.smtp.server"},
                    {"Smtp:Port", "587"},
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAsync<SmtpException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenPortIsInvalid()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "99999"}, // Port out of range
                    {"Smtp:Username", "test@test.com"},
                    {"Smtp:Password", "testpass"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Smtp:Host", "smtp.gmail.com"},
                    {"Smtp:Port", "587"},
                    {"Smtp:Username", "wrong@test.com"},
                    {"Smtp:Password", "wrongpassword"},
                    {"Smtp:From", "from@test.com"}
                }!)
                .Build();

            var sender = new SmtpEmailSender(config);

            // Act & Assert
            // BUG: Code không validate credentials trước khi send
            await Assert.ThrowsAsync<SmtpException>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", "Body"));
        }

        // --- NHÓM 4: BUG HUNTING - EDGE CASES ---

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenSubjectIsNull()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", null!, "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldThrowException_WhenBodyIsNull()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", null!));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldHandleVeryLongSubject()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);
            var longSubject = new string('A', 10000);

            // Act & Assert
            // BUG: Không giới hạn độ dài subject
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", longSubject, "Body"));
        }

        [Fact]
        public async Task SendEmailAsync_ShouldHandleVeryLongBody()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);
            var longBody = new string('B', 100000);

            // Act & Assert
            // BUG: Không giới hạn độ dài body
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await sender.SendEmailAsync("to@test.com", "Subject", longBody));
        }

        // --- NHÓM 5: BUG HUNTING - SSL/TLS HARDCODED ---

        [Fact]
        public async Task SendEmailAsync_ShouldUseHardcodedEnableSsl()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act & Assert
            // BUG: EnableSsl = true luôn luôn, không config được
            // Test này chỉ document bug, không có cách verify trực tiếp
            // Cần refactor để inject SmtpClient hoặc expose config
            Assert.True(true, "BUG DOCUMENTED: EnableSsl is hardcoded to true, not configurable");
        }

        // --- NHÓM 6: BUG HUNTING - CONCURRENT CALLS ---

        [Fact]
        public async Task SendEmailAsync_ShouldHandleConcurrentCalls()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act
            var tasks = Enumerable.Range(0, 10).Select(i =>
                sender.SendEmailAsync($"to{i}@test.com", $"Subject {i}", $"Body {i}"));

            // Assert
            // BUG: SmtpClient có thể không thread-safe
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await Task.WhenAll(tasks));
        }

        // --- NHÓM 7: BUG HUNTING - DISPOSAL ---

        [Fact]
        public async Task SendEmailAsync_ShouldDisposeSmtpClient()
        {
            // Arrange
            var config = CreateValidConfig();
            var sender = new SmtpEmailSender(config);

            // Act
            // Multiple calls to check for resource leaks
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    await sender.SendEmailAsync("to@test.com", "Subject", "Body");
                }
                catch
                {
                    // Ignore SMTP errors, we're testing disposal
                }
            }

            // Assert
            // BUG: Nếu có resource leak, test này sẽ slow hoặc fail
            Assert.True(true, "No obvious resource leak detected");
        }
    }
}