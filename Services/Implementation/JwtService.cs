using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ActosAdultosAPI.Services.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(
            IConfiguration configuration,
            UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<(string Token, DateTime ExpiresAt)>
            GenerateTokenAsync(AppUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "La clave JWT no está configurada.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer no configurado.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience no configurado.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

            if (expirationMinutes <= 0)
                throw new InvalidOperationException(
                    "La duración del JWT debe ser mayor a cero.");

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiresAt);
        }
    }
}
