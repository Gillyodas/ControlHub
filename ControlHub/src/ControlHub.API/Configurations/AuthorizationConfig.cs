using ControlHub.Infrastructure.Permissions.AuthZ;
using Microsoft.AspNetCore.Authorization;

namespace ControlHub.API.Configurations
{
    public static class AuthorizationConfig
    {
        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            // Singleton vì policy provider không inject scoped services
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Add default authorization services
            services.AddAuthorization();

            return services;
        }
    }
}