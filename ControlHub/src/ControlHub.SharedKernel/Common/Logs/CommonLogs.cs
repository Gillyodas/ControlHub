namespace ControlHub.SharedKernel.Common.Logs
{
    public class CommonLogs
    {
        // Log cho lỗi thiếu cấu hình (Admin cần fix ngay)
        public static readonly LogCode System_ConfigMissing =
            new("System.ConfigMissing", "Master Key is missing in AppSettings configuration");

        // Log cho lỗi nhập sai Master Key (Có thể là tấn công)
        public static readonly LogCode Auth_InvalidMasterKey =
            new("Auth.InvalidMasterKey", "Invalid Master Key provided during registration attempt");
    }
}
