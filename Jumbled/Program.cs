using System.Diagnostics.CodeAnalysis;
using Jumbled.Services;
using Jumbled.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jumbled;

[ExcludeFromCodeCoverage(Justification = "Host bootstrap has no branching logic to assert")]
public static class Program
{
    public static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddSingleton<IWordleAssistService, WordleAssistService>();
                services.AddHealthChecks();
            })
            .Build();

        await host.RunAsync();
    }
}
