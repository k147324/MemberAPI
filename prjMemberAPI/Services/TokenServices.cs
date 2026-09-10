using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using prjMemberAPI.Models;

namespace prjMemberAPI.Services
{
    public class TokenServices
    {
        private readonly IConfiguration _config;
        public TokenServices(IConfiguration configuration)
        {
            _config = configuration;
        }
        public async Task<string> GenerateToken(TUser u)
        {

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub,u.FId.ToString()),
                new Claim(ClaimTypes.Name,u.FUsername),
                new Claim(ClaimTypes.Role,u.FIsAdmin?"Admin":"User")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async void ValidateToken(TUser u) { 
        }
     
    }
}
