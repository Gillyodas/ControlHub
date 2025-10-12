using ControlHub.SharedKernel.Common.Errors;

namespace ControlHub.SharedKernel.Roles
{
    public static class RoleErrors
    {
        public static readonly Error RoleNameRequired =
            new("Role.NameRequired", "Role name is required.");

        public static readonly Error RoleAlreadyExists =
            new("Role.AlreadyExists", "A role with the same name already exists.");

        public static readonly Error RoleNotFound =
            new("Role.NotFound", "The specified role does not exist.");

        public static readonly Error RoleNameAlreadyExists =
            new("Role.NameAlreadyExists", "Role name already exists.");

        public static readonly Error PermissionAlreadyExists =
            new("Role.PermissionAlreadyExists", "Permission already exists in this role.");

        public static readonly Error PermissionNotFound =
            new("Role.PermissionNotFound", "Permission not found in this role.");

        public static readonly Error RoleInactive =
            new("Role.Inactive", "The role is inactive and cannot be modified.");

        public static readonly Error RoleUnexpectedError =
            new("Role.UnexpectedError", "An unexpected error occurred while processing the role.");
    }
}