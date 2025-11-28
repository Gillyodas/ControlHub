using Microsoft.AspNetCore.Authorization;

namespace ControlHub.Infrastructure.Authorization.Requirements
{
    // Đây chỉ là một "đánh dấu" (marker), không chứa logic
    public class SameUserRequirement : IAuthorizationRequirement
    {
    }
}