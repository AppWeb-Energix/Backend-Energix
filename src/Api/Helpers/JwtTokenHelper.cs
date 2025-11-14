using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Energix.API.Helpers
{
    public static class JwtTokenHelper
    {
        public static string GenerateToken(IConfiguration configuration, IEnumerable<Claim> claims)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = jwtSection["Issuer"] ?? "energix";
            var audience = jwtSection["Audience"] ?? "energix-client";
            var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60;

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}