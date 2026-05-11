using Jumbled.Services;
using Jumbled.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();
builder.Services.AddSingleton<IWordSource, FileWordSource>();
builder.Services.AddSingleton<IWordleAssistService, WordleAssistService>();
builder.Services.AddHealthChecks();

const string LocalOriginsCorsPolicy = "LocalOriginsCorsPolicy";
var allowedLocalOrigins = Environment.GetEnvironmentVariable("ALLOWED_LOCAL_ORIGINS");
if (!string.IsNullOrWhiteSpace(allowedLocalOrigins))
{
    builder.Services.AddCors(options =>
        options.AddPolicy(LocalOriginsCorsPolicy, policy =>
            policy.WithOrigins(allowedLocalOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()));
    builder.Services.AddTransient<IStartupFilter>(_ => new LocalCorsStartupFilter(LocalOriginsCorsPolicy));
}

await builder.Build().RunAsync();

sealed class LocalCorsStartupFilter(string policyName) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseCors(policyName);
        next(app);
    };
}