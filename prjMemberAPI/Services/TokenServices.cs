using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using Org.BouncyCastle.Bcpg;
using prjMemberAPI.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace prjMemberAPI.Services
{

    public class TokenServices
    {
        private readonly tempdbContext _db;
        private readonly IConfiguration _config;
        public TokenServices(IConfiguration configuration, tempdbContext db)
        {
            _db = db;
            _config = configuration;

        }
        //jwt TOKEN
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
        public async Task<string> CreateTokenAsync(int userId, string type, TimeSpan validFor)
        {
            var bytes = new byte[32];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var token = Convert.ToHexString(bytes);

            var record = new TEmailVerification
            {
                FUserId = userId,
                FToken = token,
                FType = type,
                FExpireAt = DateTime.UtcNow.Add(validFor),
                FCreatedAt = DateTime.UtcNow
            };

            _db.TEmailVerifications.Add(record);
            await _db.SaveChangesAsync();

            return token;
        }
        //other use token
        public async Task<TEmailVerification> VerifyTokenAsync(string token, string expectedType)
        {
            var record = await _db.TEmailVerifications
                .FirstOrDefaultAsync(t => t.FToken == token);

            if (record == null)
                throw new InvalidOperationException("無效的連結");
            if (record.FUsedAt != null)
                throw new InvalidOperationException("此連結已被使用過");
            if (record.FExpireAt < DateTime.UtcNow)
                throw new InvalidOperationException("連結已過期");
            if (record.FType != expectedType)
                throw new InvalidOperationException("連結類型不符");

            return record;
        }

        public async Task MarkAsUsedAsync(TEmailVerification record)
        {
            record.FUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        public TokenData GetTokenData(ClaimsPrincipal user)
        {
            return new TokenData
            {
                UserId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = user.FindFirst(ClaimTypes.Name)?.Value,
                Roles = user.FindFirst(ClaimTypes.Role)?.Value
            };
        }
    }
}
