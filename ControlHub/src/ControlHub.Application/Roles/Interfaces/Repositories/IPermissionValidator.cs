using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlHub.Application.Roles.Interfaces.Repositories
{
    public interface IPermissionValidator
    {
        Task<List<Guid>> PermissionIdsExistAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken);
    }
}
