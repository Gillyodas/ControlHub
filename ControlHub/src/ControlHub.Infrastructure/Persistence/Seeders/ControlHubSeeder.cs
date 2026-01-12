using ControlHub.Domain.Accounts.Enums;
using ControlHub.Domain.Accounts.Identifiers;
using ControlHub.Domain.Roles;
using ControlHub.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace ControlHub.Infrastructure.Persistence.Seeders
{
    public static class ControlHubSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Seed Roles
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
            }

            // Seed IdentifierConfigs
            if (!await db.IdentifierConfigs.AnyAsync())
            {
                // Email Identifier Config
                var emailConfig = IdentifierConfig.Create("Email", "Email address validation");
                emailConfig.AddRule(ValidationRuleType.Required, new Dictionary<string, object>());
                emailConfig.AddRule(ValidationRuleType.Email, new Dictionary<string, object>());
                await db.IdentifierConfigs.AddAsync(emailConfig);

                // Phone Identifier Config
                var phoneConfig = IdentifierConfig.Create("Phone", "Phone number validation");
                phoneConfig.AddRule(ValidationRuleType.Required, new Dictionary<string, object>());
                phoneConfig.AddRule(ValidationRuleType.Phone, new Dictionary<string, object>
                {
                    { "pattern", @"^(\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$" },
                    { "allowInternational", true }
                });
                await db.IdentifierConfigs.AddAsync(phoneConfig);

                // Employee ID Config
                var employeeIdConfig = IdentifierConfig.Create("EmployeeID", "Employee ID validation");
                employeeIdConfig.AddRule(ValidationRuleType.Required, new Dictionary<string, object>());
                employeeIdConfig.AddRule(ValidationRuleType.MinLength, new Dictionary<string, object> { { "length", 5 } });
                employeeIdConfig.AddRule(ValidationRuleType.MaxLength, new Dictionary<string, object> { { "length", 10 } });
                employeeIdConfig.AddRule(ValidationRuleType.Pattern, new Dictionary<string, object>
                {
                    { "pattern", @"^EMP\d{4,9}$" },
                    { "options", 0 } // RegexOptions.None
                });
                await db.IdentifierConfigs.AddAsync(employeeIdConfig);

                // Username Config
                var usernameConfig = IdentifierConfig.Create("Username", "Username validation");
                usernameConfig.AddRule(ValidationRuleType.Required, new Dictionary<string, object>());
                usernameConfig.AddRule(ValidationRuleType.MinLength, new Dictionary<string, object> { { "length", 3 } });
                usernameConfig.AddRule(ValidationRuleType.MaxLength, new Dictionary<string, object> { { "length", 20 } });
                usernameConfig.AddRule(ValidationRuleType.Custom, new Dictionary<string, object> { { "customLogic", "alphanumeric" } });
                await db.IdentifierConfigs.AddAsync(usernameConfig);

                // Age Config (Range validation example)
                var ageConfig = IdentifierConfig.Create("Age", "Age validation");
                ageConfig.AddRule(ValidationRuleType.Required, new Dictionary<string, object>());
                ageConfig.AddRule(ValidationRuleType.Range, new Dictionary<string, object>
                {
                    { "min", 18 },
                    { "max", 65 }
                });
                await db.IdentifierConfigs.AddAsync(ageConfig);

                await db.SaveChangesAsync();
            }
        }
    }
}