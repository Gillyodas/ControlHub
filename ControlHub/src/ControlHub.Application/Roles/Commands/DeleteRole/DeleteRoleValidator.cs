using FluentValidation;

namespace ControlHub.Application.Roles.Commands.DeleteRole
{
    public class DeleteRoleValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Role Id is required.");
        }
    }
}
