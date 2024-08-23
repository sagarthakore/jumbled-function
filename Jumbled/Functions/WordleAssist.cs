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
    private readonly ILogger<WordleAssist> _logger = logger;
    private readonly IWordleAssistService _wordleAssist = wordleAssist;
    private readonly TelemetryClient _telemetryClient = new(telemetryConfiguration);

    [Function("WordleAssist")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Request Received - {query}", req.Query);
        
        _telemetryClient.TrackEvent("WordleAssist Request Received", new Dictionary<string, string> {
            {
                "word", req.Query["word"]!.ToString()
            },
            {
                "exclude", req.Query["exclude"]!.ToString()
            },
            {
                "include", req.Query["include"]!.ToString()
            }
        });

        string word = req.Query["word"].ToString().ToLowerInvariant();
        string exclude = req.Query["exclude"].ToString().ToLowerInvariant();
        string include = req.Query["include"].ToString().ToLowerInvariant();

        return new OkObjectResult(_wordleAssist.GetWordGuess(word, exclude, include));
    }
}