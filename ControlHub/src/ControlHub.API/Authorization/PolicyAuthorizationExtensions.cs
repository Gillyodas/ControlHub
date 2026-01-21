using ControlHub.Application.Roles.Interfaces.Repositories;
using ControlHub.Domain.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ControlHub.API.Authorization
{
    /// <summary>
    /// Policy-based authorization requirements
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// Permission-based authorization handler
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            // Get user's permissions from their roles
            var userPermissions = await GetUserPermissionsAsync(context.User);

            if (userPermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<HashSet<string>> GetUserPermissionsAsync(System.Security.Claims.ClaimsPrincipal user)
        {
            var permissions = new HashSet<string>();

            // Get user's role claims
            var roleClaims = user.FindAll(System.Security.Claims.ClaimTypes.Role);
            
            using var scope = _serviceProvider.CreateScope();
            var roleQueries = scope.ServiceProvider.GetRequiredService<IRoleQueries>();

            foreach (var roleClaim in roleClaims)
            {
                var roleName = roleClaim.Value;
                
                // Get permissions for this role
                var rolesFound = await roleQueries.SearchByNameAsync(roleName, default);
                var role = rolesFound.FirstOrDefault(r => r.Name == roleName);

                if (role != null)
                {
                    foreach (var permission in role.Permissions)
                    {
                        permissions.Add(permission.Code);
                    }
                }
            }

            return permissions;
        }
    }

    /// <summary>
    /// Extension methods for adding policy-based authorization
    /// </summary>
    public static class PolicyAuthorizationExtensions
    {
        /// <summary>
        /// Adds policy-based authorization services
        /// </summary>
        public static IServiceCollection AddPolicyAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Add all policies
            services.AddAuthorization(options =>
            {
                // Authentication Policies
                options.AddPolicy(Policies.CanSignIn, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.signin")));
                options.AddPolicy(Policies.CanRegister, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.register")));
                options.AddPolicy(Policies.CanRefreshToken, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.refresh")));
                options.AddPolicy(Policies.CanChangePassword, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.change_password")));
                options.AddPolicy(Policies.CanForgotPassword, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.forgot_password")));
                options.AddPolicy(Policies.CanResetPassword, policy => 
                    policy.Requirements.Add(new PermissionRequirement("auth.reset_password")));

                // User Management Policies
                options.AddPolicy(Policies.CanViewUsers, policy => 
                    policy.Requirements.Add(new PermissionRequirement("users.view")));
                options.AddPolicy(Policies.CanCreateUser, policy => 
                    policy.Requirements.Add(new PermissionRequirement("users.create")));
                options.AddPolicy(Policies.CanUpdateUser, policy => 
                    policy.Requirements.Add(new PermissionRequirement("users.update")));
                options.AddPolicy(Policies.CanDeleteUser, policy => 
                    policy.Requirements.Add(new PermissionRequirement("users.delete")));
                options.AddPolicy(Policies.CanUpdateUsername, policy => 
                    policy.Requirements.Add(new PermissionRequirement("users.update_username")));

                // Role Management Policies
                options.AddPolicy(Policies.CanViewRoles, policy => 
                    policy.Requirements.Add(new PermissionRequirement("roles.view")));
                options.AddPolicy(Policies.CanCreateRole, policy => 
                    policy.Requirements.Add(new PermissionRequirement("roles.create")));
                options.AddPolicy(Policies.CanUpdateRole, policy => 
                    policy.Requirements.Add(new PermissionRequirement("roles.update")));
                options.AddPolicy(Policies.CanDeleteRole, policy => 
                    policy.Requirements.Add(new PermissionRequirement("roles.delete")));
                options.AddPolicy(Policies.CanAssignRole, policy => 
                    policy.Requirements.Add(new PermissionRequirement("roles.assign")));

                // Identifier Configuration Policies
                options.AddPolicy(Policies.CanViewIdentifierConfigs, policy => 
                    policy.Requirements.Add(new PermissionRequirement("identifiers.view")));
                options.AddPolicy(Policies.CanCreateIdentifierConfig, policy => 
                    policy.Requirements.Add(new PermissionRequirement("identifiers.create")));
                options.AddPolicy(Policies.CanUpdateIdentifierConfig, policy => 
                    policy.Requirements.Add(new PermissionRequirement("identifiers.update")));
                options.AddPolicy(Policies.CanDeleteIdentifierConfig, policy => 
                    policy.Requirements.Add(new PermissionRequirement("identifiers.delete")));
                options.AddPolicy(Policies.CanToggleIdentifierConfig, policy => 
                    policy.Requirements.Add(new PermissionRequirement("identifiers.toggle")));

                // System Administration Policies
                options.AddPolicy(Policies.CanViewSystemLogs, policy => 
                    policy.Requirements.Add(new PermissionRequirement("system.view_logs")));
                options.AddPolicy(Policies.CanViewSystemMetrics, policy => 
                    policy.Requirements.Add(new PermissionRequirement("system.view_metrics")));
                options.AddPolicy(Policies.CanManageSystemSettings, policy => 
                    policy.Requirements.Add(new PermissionRequirement("system.manage_settings")));
                options.AddPolicy(Policies.CanViewAuditLogs, policy => 
                    policy.Requirements.Add(new PermissionRequirement("system.view_audit")));

                // Profile Policies
                options.AddPolicy(Policies.CanViewOwnProfile, policy => 
                    policy.Requirements.Add(new PermissionRequirement("profile.view_own")));
                options.AddPolicy(Policies.CanUpdateOwnProfile, policy => 
                    policy.Requirements.Add(new PermissionRequirement("profile.update_own")));

                // Permission Management Policies
                options.AddPolicy(Policies.CanViewPermissions, policy => 
                    policy.Requirements.Add(new PermissionRequirement("permissions.view")));
                options.AddPolicy(Policies.CanCreatePermission, policy => 
                    policy.Requirements.Add(new PermissionRequirement("permissions.create")));
                options.AddPolicy(Policies.CanUpdatePermission, policy => 
                    policy.Requirements.Add(new PermissionRequirement("permissions.update")));
                options.AddPolicy(Policies.CanDeletePermission, policy => 
                    policy.Requirements.Add(new PermissionRequirement("permissions.delete")));
                options.AddPolicy(Policies.CanAssignPermission, policy => 
                    policy.Requirements.Add(new PermissionRequirement("permissions.assign")));
            });

            return services;
        }
    }
}
