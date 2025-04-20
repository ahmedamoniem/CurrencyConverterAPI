using FastEndpoints;
using FastEndpoints.Swagger;
using CurrencyConverter.Infrastructure.Configuration;
using CurrencyConverter.Infrastructure.Caching;
using CurrencyConverter.Application.Interfaces;
using Serilog;
using Serilog.Events;
using CurrencyConverter.Api.Middlewares;
using CurrencyConverter.Application.Factories;
using CurrencyConverter.Infrastructure.Providers;
using CurrencyConverter.Api.Middleware;
using CurrencyConverter.Application.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CurrencyConverter.Infrastructure.Security;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddHttpClientPolicies(builder.Configuration);
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddSingleton<ICacheService, RedisService>();
builder.Services.AddScoped<ICurrencyProvider, FrankfurterProvider>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddSingleton(_ =>
    new JwtTokenValidator(builder.Configuration["Jwt:Issuer"]!,
                          builder.Configuration["Jwt:Audience"]!,
                          builder.Configuration["Jwt:SecretKey"]!));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<CurrencyProviderFactory>();
builder.Host.UseSerilog((ctx, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CurrencyConverter.Api")
        .WriteTo.Console()
        .WriteTo.Seq(builder.Configuration["ConnectionStrings:Seq"] ?? "http://localhost:5341",
                     apiKey: builder.Configuration["Seq:ApiKey"],
                     restrictedToMinimumLevel: LogEventLevel.Information);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<JwtTokenValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();

public partial class Program
{
    // This class is used for testing purposes.
}   