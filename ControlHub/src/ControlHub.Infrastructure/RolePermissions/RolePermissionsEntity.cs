using ControlHub.Infrastructure.Permissions;
using ControlHub.Infrastructure.Roles;

namespace ControlHub.Infrastructure.RolePermissions
{
    public class RolePermissionEntity
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        // Navigation
        public RoleEntity Role { get; set; } = default!;
        public PermissionEntity Permission { get; set; } = default!;
    }
}