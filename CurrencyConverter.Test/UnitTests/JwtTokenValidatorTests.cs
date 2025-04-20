using CurrencyConverter.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace CurrencyConverter.Test.UnitTests;

public class JwtTokenValidatorTests
{
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const string Secret = "SuperSecretSigningKey1234567890!"; // 256-bit

    private readonly JwtTokenValidator _validator;

    public JwtTokenValidatorTests()
    {
        _validator = new JwtTokenValidator(Issuer, Audience, Secret);
    }

    private string GenerateJwtToken(string subject, bool expired = false)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var notBefore = now.AddMinutes(-2);
        var expires = expired ? now.AddMinutes(-1) : now.AddMinutes(10);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, subject),
                new Claim(ClaimTypes.Name, "TestUser")
            },
            notBefore: notBefore,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public void ValidateToken_ShouldReturnPrincipal_WhenTokenIsValid()
    {
        var token = GenerateJwtToken("user123");

        var principal = _validator.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Equal("user123", principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsExpired()
    {
        var token = GenerateJwtToken("expiredUser", expired: true);

        var principal = _validator.ValidateToken(token);

        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsTampered()
    {
        var token = GenerateJwtToken("user123") + "tamper";

        var principal = _validator.ValidateToken(token);

        Assert.Null(principal);
    }

    [Fact]
    public void IsTokenValid_ShouldReturnTrue_ForValidToken()
    {
        var token = GenerateJwtToken("validUser");

        var result = _validator.IsTokenValid(token);

        Assert.True(result);
    }

    [Fact]
    public void IsTokenValid_ShouldReturnFalse_ForInvalidToken()
    {
        var result = _validator.IsTokenValid("invalid.jwt.token");

        Assert.False(result);
    }
}
