using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Test.Helpers;

public static class JwtGenerator
{
    public static string GenerateJwtToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secure_key_123456789012345611111"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, role),
            new Claim("client_id", "integration-client")
        };

        var token = new JwtSecurityToken(
            issuer: "issuer",
            audience: "audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
