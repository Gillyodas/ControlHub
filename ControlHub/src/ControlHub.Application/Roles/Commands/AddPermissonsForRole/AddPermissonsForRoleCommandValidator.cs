using ControlHub.SharedKernel.Permissions;
using ControlHub.SharedKernel.Roles;
using FluentValidation;

namespace ControlHub.Application.Roles.Commands.SetRolePermissions
{
    public class AddPermissonsForRoleCommandValidator : AbstractValidator<AddPermissonsForRoleCommand>
    {
        public AddPermissonsForRoleCommandValidator()
        {
            RuleFor(x => x.roleId)
                .NotEmpty().WithMessage(RoleErrors.RoleIdRequired.Message);

            RuleFor(x => x.permissionIds)
                .NotEmpty().WithMessage(PermissionErrors.IdRequired.Message);
        }
    }
}
