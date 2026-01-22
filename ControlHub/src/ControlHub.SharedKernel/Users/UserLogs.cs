using ControlHub.SharedKernel.Common.Logs;

namespace ControlHub.SharedKernel.Users
{
    public static class UserLogs
    {
        public static readonly LogCode UpdateUsername_Started =
            new("User.UpdateUsername.Started", "Starting username update process");

        public static readonly LogCode UpdateUsername_NotFound =
            new("User.UpdateUsername.NotFound", "User not found for username update");

        public static readonly LogCode UpdateUsername_Failed =
            new("User.UpdateUsername.Failed", "Failed to update username");

        public static readonly LogCode UpdateUsername_Success =
            new("User.UpdateUsername.Success", "Username updated successfully");
    }
}
