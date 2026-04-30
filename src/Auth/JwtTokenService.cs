using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseAuth.Services
{
    public class JwtTokenService
    {
        private readonly string _secret;
        private readonly int _expiryMinutes;

        public JwtTokenService(string secret, int expiryMinutes = 60)
        {
            _secret = secret;
            _expiryMinutes = expiryMinutes;
        }

        // Generate a signed JWT for an authenticated enterprise user
        public string GenerateToken(string userId, string role, string tenantId)
        {
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("role", role),
                new Claim("tenantId", tenantId),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "enterprise-sso",
                audience: "enterprise-apps",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Validate token and extract principal
        public ClaimsPrincipal ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_secret);

            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = "enterprise-sso",
                ValidAudience = "enterprise-apps",
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
        }
    }
}
