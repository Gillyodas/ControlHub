namespace ControlHub.API.Roles.ViewModels.Requests
{
    public record UpdateRoleRequest(
        string Name,
        string Description
    );
}
