# TestDataProvider - ControlHub

## 📋 DANH SÁCH TÀI KHOẢN TEST

### 🔑 Tài khoản chính (Basic Test Accounts)

| **Loại** | **Identifier** | **Type** | **Password** | **Role** |
|---------|----------------|----------|-------------|----------|
| SuperAdmin | `gillyodaswork@gmail.com` | Email (0) | `Admin@123` | SuperAdmin |
| Admin | `admin123` | Username (2) | `Admin@123` | Admin |
| User 1 | `EMP00001` | EmployeeID (2) | `User@123` | User |
| User 2 | `+84123456789` | Phone (1) | `User@123` | User |

### 🧪 Tài khoản mở rộng (Extended Test Accounts)

| **Loại** | **Identifier** | **Type** | **Password** | **Role** |
|---------|----------------|----------|-------------|----------|
| User | `testuser` | Username (2) | `Test@123` | User |
| User | `test@example.com` | Email (0) | `Test@123` | User |
| Admin | `testadmin` | Username (2) | `Admin@123` | Admin |

---

## 🚀 SỬ DỤNG TEST DATA PROVIDER

### 1. Trong Seeder (Mặc định)

```csharp
// ControlHubSeeder.cs - Đã được cập nhật để sử dụng TestDataProvider
public static async Task SeedAsync(AppDbContext db)
{
    // Seed Roles
    if (!await db.Roles.AnyAsync())
    {
        // ... seed roles
    }

    // Seed IdentifierConfigs using TestDataProvider
    await TestDataProvider.SeedTestIdentifierConfigsAsync(db);

    // Seed Test Accounts using TestDataProvider
    if (!await db.Accounts.AnyAsync())
    {
        await TestDataProvider.SeedTestAccountsAsync(db, includeExtended: false);
    }
}
```

### 2. Seed thêm tài khoản mở rộng

```csharp
// Để seed thêm tài khoản mở rộng
await TestDataProvider.SeedTestAccountsAsync(db, includeExtended: true);
```

### 3. Lấy thông tin tài khoản test

```csharp
// Lấy tài khoản theo identifier
var account = TestDataProvider.GetTestAccount("gillyodaswork@gmail.com");
if (account != null)
{
    Console.WriteLine($"Role: {account.Role}, Password: {account.Password}");
}

// Lấy tất cả tài khoản theo role
var adminAccounts = TestDataProvider.GetTestAccountsByRole("Admin");
foreach (var admin in adminAccounts)
{
    Console.WriteLine($"Admin: {admin.IdentifierValue}");
}
```

---

## 🧪 API TEST EXAMPLES

### 1. SignIn Tests

```bash
# SuperAdmin - Email
POST http://localhost:5087/api/auth/signin
{
  "value": "gillyodaswork@gmail.com",
  "password": "Admin@123",
  "type": 0
}

# Admin - Username
POST http://localhost:5087/api/auth/signin
{
  "value": "admin123",
  "password": "Admin@123",
  "type": 2
}

# User - EmployeeID
POST http://localhost:5087/api/auth/signin
{
  "value": "EMP00001",
  "password": "User@123",
  "type": 2
}

# User - Phone
POST http://localhost:5087/api/auth/signin
{
  "value": "+84123456789",
  "password": "User@123",
  "type": 1
}
```

### 2. Register Tests

```bash
# Register new user with Email
POST http://localhost:5087/api/auth/register/user
{
  "value": "newuser@example.com",
  "type": 0,
  "password": "User@123"
}

# Register new user with EmployeeID
POST http://localhost:5087/api/auth/register/user
{
  "value": "EMP00002",
  "type": 2,
  "password": "User@123"
}
```

### 3. IdentifierConfig Tests

```bash
# Get all configs
GET http://localhost:5087/api/Identifier
Authorization: Bearer {token}

# Get active configs (no auth required)
GET http://localhost:5087/api/Identifier/active

# Create new config
POST http://localhost:5087/api/Identifier
Authorization: Bearer {token}
{
  "name": "StudentID",
  "description": "Student ID validation",
  "rules": [
    {
      "type": 3,
      "parameters": { "pattern": "^STU\\d{6}$" },
      "errorMessage": "Invalid format",
      "order": 1
    }
  ]
}
```

---

## 🔧 CUSTOM TEST DATA

### Thêm tài khoản test mới

```csharp
// Thêm vào TestDataProvider.TestAccounts list
new TestAccount
{
    Role = "User",
    IdentifierType = IdentifierType.Username,
    IdentifierConfigName = "Username",
    IdentifierValue = "mytestuser",
    Password = "MyTest@123",
    UserName = "My Test User"
}
```

### Thêm password hash mới

```csharp
// Trong TestPasswordHasher constructor
_hashedPasswords["MyTest@123"] = (
    new byte[] { /* your hash bytes */ },
    new byte[] { /* your salt bytes */ }
);
```

---

## 📝 IdentifierType Constants

```csharp
public enum IdentifierType
{
    Email = 0,
    Phone = 1,
    Username = 2
}
```

---

## 🎯 TESTING CHECKLIST

- [ ] SignIn với tất cả 4 tài khoản chính
- [ ] Register user mới với các identifier types
- [ ] Test IdentifierConfig CRUD operations
- [ ] Test pattern validation
- [ ] Test authorization (SuperAdmin > Admin > User)
- [ ] Test frontend UI với các tài khoản
- [ ] Test password verification functionality

---

## 🐛 TROUBLESHOOTING

### Password không hoạt động
1. Kiểm tra `TestPasswordHasher` có hash cho password đó không
2. Verify password trong database matches expected hash
3. Test với `Admin@123` và `User@123` (đã được pre-hashed)

### Identifier không tìm thấy
1. Kiểm tra `IdentifierConfigName` có tồn tại trong database không
2. Verify `IdentifierType` enum value đúng
3. Test với các tài khoản đã biết hoạt động

### Authorization failed
1. Kiểm tra role assignment trong database
2. Verify token generation and validation
3. Test với SuperAdmin account (`gillyodaswork@gmail.com`)
