namespace ControlHub.API.Roles.ViewModels.Requests
{
    public class AddPermissonsForRoleRequest
    {
        public string RoleId { get; set; } = null!;
        public IEnumerable<string> PermissionIds { get; set; } = null!;
    }
}
