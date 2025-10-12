using ControlHub.Infrastructure.RolePermissions;

namespace ControlHub.Infrastructure.Permissions
{
    public class PermissionEntity
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Description { get; set; } = string.Empty;

        // Navigation
        public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    }
}