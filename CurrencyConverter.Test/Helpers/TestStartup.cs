using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Application.Factories;
using CurrencyConverter.Infrastructure.Providers;
using CurrencyConverter.Infrastructure.Caching;
using CurrencyConverter.Infrastructure.Security;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;

namespace CurrencyConverter.Test.Helpers;

public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register app dependencies
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ICurrencyProvider, FrankfurterProvider>();
        services.AddScoped<ICacheService, RedisService>();
        services.AddScoped<CurrencyProviderFactory>();

        // Register JwtTokenValidator if used
        services.AddSingleton<JwtTokenValidator>(_ =>
            new JwtTokenValidator("issuer", "audience", "test-secret-key"));

        // Setup authentication for testing
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "issuer",
                        ValidAudience = "audience",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key"))
                    };
                });

        services.AddAuthorization();

        services.AddFastEndpoints()
                .SwaggerDocument();
    }

    public static void Configure(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();
    }
}
