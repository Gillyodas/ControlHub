namespace ControlHub.API.Roles.ViewModels.Requests
{
    public class AddPermissonsForRoleRequest
    {
        public IEnumerable<string> PermissionIds { get; set; } = null!;
    }
}
