using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlHub.Application.Roles.DTOs
{
    public record CreateRoleDto(
        string Name,
        string? Description,
        IEnumerable<Guid>? PermissionIds
    );
}
