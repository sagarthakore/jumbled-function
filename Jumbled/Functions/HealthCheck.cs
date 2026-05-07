using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jumbled.Functions;

public sealed class HealthCheck(HealthCheckService healthCheckService)
{
    [Function("HealthCheck")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var healthReport = await healthCheckService.CheckHealthAsync(req.HttpContext.RequestAborted);

        var response = new
        {
            status = healthReport.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration
            })
        };

        var statusCode = healthReport.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return new ObjectResult(response) { StatusCode = statusCode };
    }
}
