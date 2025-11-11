using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ControlHub.Infrastructure.Tokens
{
    // Lớp này sẽ tự động được gọi bởi AddJwtBearer
    public class ConfigureJwtBearerOptions : IConfigureOptions<JwtBearerOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureJwtBearerOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(JwtBearerOptions options)
        {
            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is missing.");
            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
            };
        }
    }
}