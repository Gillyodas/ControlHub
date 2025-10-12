using FluentValidation;

namespace ControlHub.Application.Roles.Commands.CreateRoles
{
    public class CreateRolesCommandValidator : AbstractValidator<CreateRolesCommand>
    {
        public CreateRolesCommandValidator()
        {
            // Kiểm tra danh sách Roles không null hoặc rỗng
            RuleFor(x => x.Roles)
                .NotNull().WithMessage("Roles list is required.")
                .Must(r => r.Any()).WithMessage("At least one role must be provided.");

            // Validate từng phần tử trong danh sách Roles
            RuleForEach(x => x.Roles)
                .SetValidator(new CreateRoleDtoValidator());
        }
    }
}
