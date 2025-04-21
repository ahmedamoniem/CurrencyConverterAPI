using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

var builder = DistributedApplication.CreateBuilder(args);

;
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var cache = builder.AddRedis("cache")
                   .WithRedisInsight();

var seq = builder.AddSeq("seq")
                 .ExcludeFromManifest()
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithEnvironment("ACCEPT_EULA", "Y");

builder.AddProject<Projects.CurrencyConverter_API>("currencyconverter-api")
       .WithReference(cache)
       .WithReference(seq)
       .WaitFor(seq);

builder.Build().Run();
