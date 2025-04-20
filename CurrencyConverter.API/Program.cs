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
app.UseHttpsRedirection();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<JwtTokenValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();