namespace ControlHub.Domain.Permissions
{
    /// <summary>
    /// Static permissions for the ControlHub system
    /// </summary>
    public static class Permissions
    {
        // Authentication Permissions
        public const string SignIn = "permissions.auth.signin";
        public const string Register = "permissions.auth.register";
        public const string RefreshToken = "permissions.auth.refresh";
        public const string ChangePassword = "permissions.auth.change_password";
        public const string ForgotPassword = "permissions.auth.forgot_password";
        public const string ResetPassword = "permissions.auth.reset_password";

        // User Management Permissions
        public const string ViewUsers = "permissions.users.view";
        public const string CreateUser = "permissions.users.create";
        public const string UpdateUser = "permissions.users.update";
        public const string DeleteUser = "permissions.users.delete";
        public const string UpdateUsername = "permissions.users.update_username";

        // Role Management Permissions
        public const string ViewRoles = "permissions.roles.view";
        public const string CreateRole = "permissions.roles.create";
        public const string UpdateRole = "permissions.roles.update";
        public const string DeleteRole = "permissions.roles.delete";
        public const string AssignRole = "permissions.roles.assign";

        // Identifier Configuration Permissions
        public const string ViewIdentifierConfigs = "permissions.identifiers.view";
        public const string CreateIdentifierConfig = "permissions.identifiers.create";
        public const string UpdateIdentifierConfig = "permissions.identifiers.update";
        public const string DeleteIdentifierConfig = "permissions.identifiers.delete";
        public const string ToggleIdentifierConfig = "permissions.identifiers.toggle";

        // System Administration Permissions
        public const string ViewSystemLogs = "permissions.system.view_logs";
        public const string ViewSystemMetrics = "permissions.system.view_metrics";
        public const string ManageSystemSettings = "permissions.system.manage_settings";
        public const string ViewAuditLogs = "permissions.system.view_audit_logs";

        // Profile Permissions (all users can manage their own profile)
        public const string ViewOwnProfile = "permissions.profile.view_own";
        public const string UpdateOwnProfile = "permissions.profile.update_own";

        // Permission Management (SuperAdmin only)
        public const string ViewPermissions = "permissions.permissions.view";
        public const string CreatePermission = "permissions.permissions.create";
        public const string UpdatePermission = "permissions.permissions.update";
        public const string DeletePermission = "permissions.permissions.delete";
        public const string AssignPermission = "permissions.permissions.assign";
    }

    /// <summary>
    /// Permission policies for authorization
    /// </summary>
    public static class Policies
    {
        public const string CanSignIn = "CanSignIn";
        public const string CanRegister = "CanRegister";
        public const string CanRefreshToken = "CanRefreshToken";
        public const string CanChangePassword = "CanChangePassword";
        public const string CanForgotPassword = "CanForgotPassword";
        public const string CanResetPassword = "CanResetPassword";

        public const string CanViewUsers = "CanViewUsers";
        public const string CanCreateUser = "CanCreateUser";
        public const string CanUpdateUser = "CanUpdateUser";
        public const string CanDeleteUser = "CanDeleteUser";
        public const string CanUpdateUsername = "CanUpdateUsername";

        public const string CanViewRoles = "CanViewRoles";
        public const string CanCreateRole = "CanCreateRole";
        public const string CanUpdateRole = "CanUpdateRole";
        public const string CanDeleteRole = "CanDeleteRole";
        public const string CanAssignRole = "CanAssignRole";

        public const string CanViewIdentifierConfigs = "CanViewIdentifierConfigs";
        public const string CanCreateIdentifierConfig = "CanCreateIdentifierConfig";
        public const string CanUpdateIdentifierConfig = "CanUpdateIdentifierConfig";
        public const string CanDeleteIdentifierConfig = "CanDeleteIdentifierConfig";
        public const string CanToggleIdentifierConfig = "CanToggleIdentifierConfig";

        public const string CanViewSystemLogs = "CanViewSystemLogs";
        public const string CanViewSystemMetrics = "CanViewSystemMetrics";
        public const string CanManageSystemSettings = "CanManageSystemSettings";
        public const string CanViewAuditLogs = "CanViewAuditLogs";

        public const string CanViewOwnProfile = "CanViewOwnProfile";
        public const string CanUpdateOwnProfile = "CanUpdateOwnProfile";

        public const string CanViewPermissions = "CanViewPermissions";
        public const string CanCreatePermission = "CanCreatePermission";
        public const string CanUpdatePermission = "CanUpdatePermission";
        public const string CanDeletePermission = "CanDeletePermission";
        public const string CanAssignPermission = "CanAssignPermission";
    }
}
