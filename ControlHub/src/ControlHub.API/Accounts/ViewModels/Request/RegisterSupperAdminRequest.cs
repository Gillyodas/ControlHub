namespace ControlHub.API.Accounts.ViewModels.Request
{
    public class RegisterSupperAdminRequest
    {
        public string Value { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string MasterKey { get; set; } = null!;
    }
}
