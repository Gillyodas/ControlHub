using ControlHub.SharedKernel.Common.Logs;

namespace ControlHub.SharedKernel.Accounts
{
    public static class AccountLogs
    {
        // RegisterUser
        public static readonly LogCode RegisterUser_Started =
            new("Account.RegisterUser.Started", "Starting account creation");

        public static readonly LogCode RegisterUser_InvalidIdentifier =
            new("Account.RegisterUser.InvalidIdentifier", "Invalid identifier format");

        public static readonly LogCode RegisterUser_IdentifierExists =
            new("Account.RegisterUser.IdentifierExists", "Identifier already exists");

        public static readonly LogCode RegisterUser_FactoryFailed =
            new("Account.RegisterUser.FactoryFailed", "Account factory failed");

        public static readonly LogCode RegisterUser_Success =
            new("Account.RegisterUser.Success", "Account created successfully");

        // RegisterAdmin
        public static readonly LogCode RegisterAdmin_Started =
            new("Account.RegisterAdmin.Started", "Starting account creation");

        public static readonly LogCode RegisterAdmin_InvalidIdentifier =
            new("Account.RegisterAdmin.InvalidIdentifier", "Invalid identifier format");

        public static readonly LogCode RegisterAdmin_IdentifierExists =
            new("Account.RegisterAdmin.IdentifierExists", "Identifier already exists");

        public static readonly LogCode RegisterAdmin_FactoryFailed =
            new("Account.RegisterAdmin.FactoryFailed", "Account factory failed");

        public static readonly LogCode RegisterAdmin_Success =
            new("Account.RegisterAdmin.Success", "Account created successfully");

        // RegisterSupperAdmin
        public static readonly LogCode RegisterSupperAdmin_Started =
            new("Account.RegisterSupperAdmin.Started", "Starting account creation");

        public static readonly LogCode RegisterSupperAdmin_InvalidIdentifier =
            new("Account.RegisterSupperAdmin.InvalidIdentifier", "Invalid identifier format");

        public static readonly LogCode RegisterSupperAdmin_IdentifierExists =
            new("Account.RegisterSupperAdmin.IdentifierExists", "Identifier already exists");

        public static readonly LogCode RegisterSupperAdmin_FactoryFailed =
            new("Account.RegisterSupperAdmin.FactoryFailed", "Account factory failed");

        public static readonly LogCode RegisterSupperAdmin_Success =
            new("Account.RegisterSupperAdmin.Success", "Account created successfully");

        // ChangePassword
        public static readonly LogCode ChangePassword_Started =
            new("Auth.ChangePassword.Started", "Starting change password flow");

        public static readonly LogCode ChangePassword_AccountNotFound =
            new("Auth.ChangePassword.AccountNotFound", "Account not found");

        public static readonly LogCode ChangePassword_InvalidPassword =
            new("Auth.ChangePassword.InvalidPassword", "Invalid current password");

        public static readonly LogCode ChangePassword_Success =
            new("Auth.ChangePassword.Success", "Password changed successfully");

        public static readonly LogCode ChangePassword_UpdateFailed =
            new("Account.ChangePassword.UpdateFailed", "Failed to update password");

        // ForgotPassword
        public static readonly LogCode ForgotPassword_Started =
            new("Account.ForgotPassword.Started", "Starting forgot password flow");

        public static readonly LogCode ForgotPassword_InvalidIdentifier =
            new("Account.ForgotPassword.InvalidIdentifier", "Invalid identifier format");

        public static readonly LogCode ForgotPassword_IdentifierNotFound =
            new("Account.ForgotPassword.IdentifierNotFound", "Identifier not found");

        public static readonly LogCode ForgotPassword_TokenGenerated =
            new("Account.ForgotPassword.TokenGenerated", "Password reset token generated");

        public static readonly LogCode ForgotPassword_NotificationSent =
            new("Account.ForgotPassword.NotificationSent", "Password reset notification sent successfully");

        // SignIn
        public static readonly LogCode SignIn_Started =
            new("Account.SignIn.Started", "Starting sign-in process");

        public static readonly LogCode SignIn_InvalidIdentifier =
            new("Account.SignIn.InvalidIdentifier", "Invalid identifier format");

        public static readonly LogCode SignIn_AccountNotFound =
            new("Account.SignIn.AccountNotFound", "Account not found");

        public static readonly LogCode SignIn_InvalidPassword =
            new("Account.SignIn.InvalidPassword", "Invalid password");

        public static readonly LogCode SignIn_Success =
            new("Account.SignIn.Success", "User signed in successfully");

        // ResetPassword
        public static readonly LogCode ResetPassword_Started =
        new("Account.ResetPassword.Started", "Starting reset password flow");

        public static readonly LogCode ResetPassword_TokenNotFound =
            new("Account.ResetPassword.TokenNotFound", "Reset token not found");

        public static readonly LogCode ResetPassword_TokenInvalid =
            new("Account.ResetPassword.TokenInvalid", "Reset token is invalid");

        public static readonly LogCode ResetPassword_TokenMismatch =
            new("Account.ResetPassword.TokenMismatch", "Reset token does not belong to account");

        public static readonly LogCode ResetPassword_AccountNotFound =
            new("Account.ResetPassword.AccountNotFound", "Account not found");

        public static readonly LogCode ResetPassword_AccountDisabled =
            new("Account.ResetPassword.AccountDisabled", "Account is disabled");

        public static readonly LogCode ResetPassword_PasswordHashFailed =
            new("Account.ResetPassword.PasswordHashFailed", "Failed to hash the new password");

        public static readonly LogCode ResetPassword_Success =
            new("Account.ResetPassword.Success", "Password reset successfully");

        // LogOut
        public static readonly LogCode SignOut_Started =
            new("Account.SignOut.Started", "Starting sign-out process");

        public static readonly LogCode SignOut_InvalidToken =
            new("Account.SignOut.InvalidToken", "Access token is invalid");

        public static readonly LogCode SignOut_TokenNotFound =
            new("Account.SignOut.TokenNotFound", "Token not found in database");

        public static readonly LogCode SignOut_InvalidAccountId =
            new("Account.SignOut.InvalidAccountId", "Invalid account ID extracted from token");

        public static readonly LogCode SignOut_MismatchedAccount =
            new("Account.SignOut.MismatchedAccount", "Access and refresh token belong to different accounts");

        public static readonly LogCode SignOut_TokenAlreadyRevoked =
            new("Account.SignOut.AlreadyRevoked", "Token already revoked before logout");

        public static readonly LogCode SignOut_Success =
            new("Account.SignOut.Success", "User signed out successfully");
    }
}