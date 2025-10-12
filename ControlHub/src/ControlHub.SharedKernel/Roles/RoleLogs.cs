using ControlHub.SharedKernel.Common.Logs;

namespace ControlHub.SharedKernel.Roles
{
    public static class RoleLogs
    {
        public static readonly LogCode CreateRoles_Started =
            new("Role.CreateRoles.Started", "Starting role creation process");

        public static readonly LogCode CreateRoles_DuplicateNames =
            new("Role.CreateRoles.DuplicateNames", "Duplicate role names detected");

        public static readonly LogCode CreateRoles_Success =
            new("Role.CreateRoles.Success", "Roles created successfully");

        public static readonly LogCode CreateRoles_Failed =
            new("Role.CreateRoles.Failed", "Error occurred while creating roles");
    }
}