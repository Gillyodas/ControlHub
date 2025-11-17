namespace ControlHub.API.Accounts.ViewModels.Request
{
    public class RegisterAdminRequest
    {
        public string Value { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
