using CurrencyConverter.Infrastructure.Security;
using Serilog;

namespace CurrencyConverter.Api.Middlewares;

public class JwtTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenValidator _tokenValidator;
    private readonly Serilog.ILogger _logger;

    public JwtTokenValidationMiddleware(RequestDelegate next, JwtTokenValidator tokenValidator)
    {
        _next = next;
        _tokenValidator = tokenValidator;
        _logger = Log.ForContext<JwtTokenValidationMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? authHeader = context.Request.Headers.Authorization;

        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            string token = authHeader["Bearer ".Length..].Trim();

            var principal = _tokenValidator.ValidateToken(token);

            if (principal is not null)
            {
                _logger.Information("JWT validated for user: {User}", principal.Identity?.Name);
                context.User = principal;
            }
            else
            {
                _logger.Warning("Invalid or expired JWT received");
            }
        }
        else
        {
            _logger.Debug("No Bearer token found in request headers.");
        }

        await _next(context);
    }
}
