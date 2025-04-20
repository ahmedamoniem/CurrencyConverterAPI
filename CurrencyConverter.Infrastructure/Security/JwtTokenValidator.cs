using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Infrastructure.Security;

public class JwtTokenValidator(string issuer, string audience, string signingKey)
{
    private readonly TokenValidationParameters _validationParameters = new()
    {
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero
    };
    private readonly ILogger _logger = Log.ForContext<JwtTokenValidator>();


    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, _validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Warning("Invalid algorithm on token: {Alg}", ((JwtSecurityToken)validatedToken).Header.Alg);
                return null;
            }

            _logger.Information("JWT validated for subject: {Subject}", principal.Identity?.Name);
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.Warning(ex, "JWT expired");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.Warning(ex, "Invalid JWT");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during token validation");
            return null;
        }
    }

    public bool IsTokenValid(string token) => ValidateToken(token) != null;
}
