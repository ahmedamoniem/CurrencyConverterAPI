var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CurrencyConverter_API>("currencyconverter-api");

builder.Build().Run();
