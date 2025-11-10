using ControlHub.Infrastructure.RolePermissions;
using ControlHub.Infrastructure.Accounts;

namespace ControlHub.Infrastructure.Roles
{
    public class RoleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Navigation
        public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();
        public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
    }
}