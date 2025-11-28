using Microsoft.AspNetCore.Authorization;

namespace ControlHub.Infrastructure.Authorization.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
