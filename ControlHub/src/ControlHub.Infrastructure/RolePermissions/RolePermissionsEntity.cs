using ControlHub.Domain.Permissions;
using ControlHub.Domain.Roles;

namespace ControlHub.Infrastructure.RolePermissions
{
    // Class này thuần túy là POCO cho bảng trung gian (Join Table)
    public class RolePermissionEntity
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        public Role Role { get; set; } = default!;
        public Permission Permission { get; set; } = default!;
    }
}