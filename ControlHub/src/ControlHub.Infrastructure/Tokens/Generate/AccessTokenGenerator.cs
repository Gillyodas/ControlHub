using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ControlHub.Application.Tokens.Interfaces.Generate;
using Microsoft.Extensions.Configuration;

namespace ControlHub.Infrastructure.Tokens.Generate
{
    public class AccessTokenGenerator : TokenGeneratorBase, IAccessTokenGenerator
    {
        public AccessTokenGenerator(IConfiguration config) : base(config) { }

        public string Generate(string accountId, string identifier, string roleId)
        {
            var claims = new List<Claim>
            {
                // "sub" là định danh duy nhất của user theo chuẩn JWT
                new Claim(JwtRegisteredClaimNames.Sub, accountId),

                // Thêm NameIdentifier cho dễ lấy ở HttpContext.User
                new Claim(ClaimTypes.NameIdentifier, identifier),

                // Mỗi token nên có JTI (JWT ID) để phân biệt, phục vụ revoke nếu cần
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // Thêm role claim
                new Claim(ClaimTypes.Role, roleId)
            };

            // TTL 15 phút
            return GenerateToken(claims, TimeSpan.FromMinutes(15));
        }
    }
}