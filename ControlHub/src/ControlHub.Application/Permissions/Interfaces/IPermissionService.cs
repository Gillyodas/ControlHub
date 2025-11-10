<<<<<<< HEAD
﻿namespace ControlHub.Application.Permissions.Interfaces
=======
﻿using ControlHub.Domain.Permissions;

namespace ControlHub.Application.Permissions.Interfaces
>>>>>>> feature/auth/claims-enrichment
{
    public interface IPermissionService
    {
        Task<IEnumerable<string>> GetPermissionsForRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
