ControlHub - Identity & Access Management SolutionControlHub là thư viện .NET 8 trọn gói cung cấp giải pháp xác thực (Authentication), phân quyền (RBAC), và quản lý người dùng (User Management) chuẩn Clean Architecture.📦 Cài đặtCài đặt gói NuGet vào dự án ASP.NET Core của bạn bằng lệnh:dotnet add package ControlHub.Core
🚀 Hướng dẫn tích hợp (Quick Start)Bước 1: Cấu hình appsettings.json (BẮT BUỘC)Copy đoạn cấu hình dưới đây vào file appsettings.json của dự án. Hãy thay đổi các giá trị trong ngoặc <...> cho phù hợp với môi trường của bạn.{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  // 1. Cấu hình Database (SQL Server)
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ControlHub_AuthDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },

  // 2. Cấu hình JWT (Token)
  "Jwt": {
    "Issuer": "ControlHub",
    "Audience": "ControlHubUsers",
    "Key": "<YOUR_SECRET_KEY_MUST_BE_AT_LEAST_32_CHARS_LONG>" 
    // ⚠️ Key phải dài ít nhất 32 ký tự
  },

  // 3. Cấu hình thời hạn Token
  "TokenSettings": {
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 14,
    "ResetPasswordMinutes": 30,
    "VerifyEmailHours": 24
  },

  // 4. Mật khẩu ứng dụng (Dùng để tạo SuperAdmin ban đầu)
  "AppPassword": {
    "MasterKey": "<YOUR_SECURE_MASTER_KEY>"
  },

  // 5. Cấu hình Email (SMTP)
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "<your_email@gmail.com>",
    "Password": "<your_app_password>",
    "From": "<your_email@gmail.com>"
  },

  // 6. Cấu hình đường dẫn Client
  "BaseUrl": {
    "ClientBaseUrl": "[https://your-frontend-domain.com](https://your-frontend-domain.com)",
    "DevBaseUrl": "https://localhost:7110"
  },

  // 7. Cấu hình Role ID (Tùy chọn)
  "RoleSettings": {
    "SuperAdminRoleId": "9BA459E9-2A98-43C4-8530-392A63C66F1B",
    "AdminRoleId": "0CD24FAC-ABD7-4AD9-A7E4-248058B8D404",
    "UserRoleId": "8CF94B41-5AD8-4893-82B2-B193C91717AF"
  }
}
Bước 2: Tích hợp trong Program.csMở file Program.cs, import namespace và thêm 2 dòng lệnh quan trọng (AddControlHub và UseControlHub).using ControlHub; // 1. Import namespace

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// A. ĐĂNG KÝ DỊCH VỤ (Service Registration)
// =============================================================

// Tự động đăng ký: EF Core, JWT Auth, MediatR, Swagger Config, Repositories...
builder.Services.AddControlHub(builder.Configuration);

// (Các cấu hình khác của app chính...)
builder.Services.AddEndpointsApiExplorer();
// Lưu ý: ControlHub đã tự động cấu hình Swagger Gen (Bearer Auth).
// Bạn KHÔNG CẦN gọi builder.Services.AddSwaggerGen() nữa trừ khi muốn override.

var app = builder.Build();

// =============================================================
// B. KÍCH HOẠT PIPELINE (Middleware & Data)
// =============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Bắt buộc phải có Auth Middleware của App chính
app.UseAuthentication();
app.UseAuthorization();

// Kích hoạt ControlHub:
// - Tự động chạy Migration tạo bảng (Schema: ControlHub.*)
// - Tự động Seed dữ liệu Roles & SuperAdmin mặc định
app.UseControlHub();

app.MapControllers();

app.Run();
🛠 Xử lý sự cố thường gặp (Troubleshooting)1. Lỗi More than one DbContext was foundKhi bạn chạy lệnh tạo Migration cho ứng dụng của mình (ví dụ QuanLyPhongTro), EF Core thấy có 2 DbContext: ApplicationDbContext (của bạn) và AppDbContext (của ControlHub).Khắc phục: Bạn cần chỉ định rõ DbContext nào bằng tham số --context:# Tạo migration cho DB của bạn
dotnet ef migrations add Init_MyDb --context YourApplicationDbContext

# Update DB của bạn
dotnet ef database update --context YourApplicationDbContext
(Thay YourApplicationDbContext bằng tên class DbContext trong dự án của bạn).2. Lỗi Signature validation failed (IDX10500)Nguyên nhân: Chưa cấu hình Jwt:Key hoặc Key quá ngắn (dưới 32 ký tự).Khắc phục: Kiểm tra lại file appsettings.json.3. Không thấy bảng trong DatabaseNguyên nhân: Connection String sai hoặc chưa chạy app.UseControlHub().Khắc phục: ControlHub sử dụng Schema riêng (ControlHub.Users, ControlHub.Roles...). Hãy kiểm tra kỹ trong SQL Server dưới mục Tables (có thể cần Refresh).4. Lỗi Unable to resolve service...Nguyên nhân: Thiếu file DLL hoặc chưa gọi builder.Services.AddControlHub(...).Khắc phục: Clean Solution và Rebuild lại project.