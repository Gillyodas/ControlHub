using ControlHub.Domain.Roles;
using ControlHub.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.Persistence.Seeders
{
    public static class ControlHubSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Kiểm tra nếu chưa có bất kỳ Role nào thì mới Seed
            if (!await db.Roles.AnyAsync())
            {
                var superAdmin = Role.Create(
                    ControlHubDefaults.Roles.SuperAdminId, // Dùng ID cố định
                    ControlHubDefaults.Roles.SuperAdminName,
                    "System Super Admin");

                var admin = Role.Create(
                    ControlHubDefaults.Roles.AdminId, // Dùng ID cố định
                    ControlHubDefaults.Roles.AdminName,
                    "System Admin");

                var user = Role.Create(
                    ControlHubDefaults.Roles.UserId, // Dùng ID cố định
                    ControlHubDefaults.Roles.UserName,
                    "Standard User");

                await db.Roles.AddRangeAsync(superAdmin, admin, user);
                await db.SaveChangesAsync();
            }
        }
    }
}