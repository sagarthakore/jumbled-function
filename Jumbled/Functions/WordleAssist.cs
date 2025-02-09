using Jumbled.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jumbled.Functions;

public class WordleAssist(ILogger<WordleAssist> logger, IWordleAssistService wordleAssist, TelemetryConfiguration telemetryConfiguration)
{
    private readonly TelemetryClient _telemetryClient = new(telemetryConfiguration);

    [Function("WordleAssist")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        logger.LogInformation("Request Received - {query}", req.Query);

        var queryParams = ExtractQueryParameters(req);
        TrackTelemetryEvent(queryParams);

        return new OkObjectResult(wordleAssist.GetWordGuess(queryParams.word, queryParams.exclude, queryParams.include));
    }

    private static (string word, string exclude, string include) ExtractQueryParameters(HttpRequest req)
    {
        var word = req.Query["word"].ToString().ToLowerInvariant();
        var exclude = req.Query["exclude"].ToString().ToLowerInvariant();
        var include = req.Query["include"].ToString().ToLowerInvariant();

        return (word, exclude, include);
    }

    private void TrackTelemetryEvent((string word, string exclude, string include) queryParams)
    {
        _telemetryClient.TrackEvent("Request Received", new Dictionary<string, string> {
            { "word", queryParams.word },
            { "exclude", queryParams.exclude },
            { "include", queryParams.include }
        });
    }
}