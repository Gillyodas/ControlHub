namespace ControlHub.Application.Users.DTOs
{
    public record UserDto(
        Guid Id,
        string Username,
        string? Email,
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        bool IsActive,
        Guid RoleId,
        string? RoleName
    );
}
