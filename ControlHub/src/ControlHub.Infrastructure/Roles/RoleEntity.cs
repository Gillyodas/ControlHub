<<<<<<< HEAD
<<<<<<< Updated upstream
﻿using ControlHub.Infrastructure.AccountRoles;
=======
﻿using ControlHub.Infrastructure.Accounts;
>>>>>>> Stashed changes
using ControlHub.Infrastructure.RolePermissions;
=======
﻿using ControlHub.Infrastructure.RolePermissions;
using ControlHub.Infrastructure.Accounts;
>>>>>>> feature/auth/claims-enrichment

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