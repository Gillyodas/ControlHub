using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace ControlHub.Application.Tokens.Interfaces
{
    public interface ITokenVerifier
    {
        ClaimsPrincipal? Verify(string token);
    }
}
