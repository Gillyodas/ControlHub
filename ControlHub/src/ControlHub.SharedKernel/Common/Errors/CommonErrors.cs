namespace ControlHub.SharedKernel.Common.Errors
{
    public static class CommonErrors
    {
        // Lỗi 500: Lỗi cấu hình server
        public static readonly Error SystemConfigurationError = Error.Failure(
            "System.ConfigurationError", "System configuration is invalid. Please contact administrator.");

        // Lỗi 403: Sai Master Key
        public static readonly Error InvalidMasterKey = Error.Forbidden(
            "Auth.InvalidMasterKey", "The provided Master Key is incorrect.");
    }
}
