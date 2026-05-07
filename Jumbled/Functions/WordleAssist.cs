using Jumbled.Models;
using Jumbled.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jumbled.Functions;

public sealed partial class WordleAssist(ILogger<WordleAssist> logger, IWordleAssistService wordleAssist, TelemetryClient telemetryClient)
{
    [Function("WordleAssist")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var request = ExtractQueryParameters(req);

        if (!TryValidate(request, out var error))
        {
            return new BadRequestObjectResult(new { error });
        }

        LogRequestReceived(logger, request);
        TrackTelemetryEvent(request);

        return new OkObjectResult(wordleAssist.GetWordGuess(request));
    }

    private void TrackTelemetryEvent(WordleAssistRequest request)
    {
        telemetryClient.TrackEvent("Request Received", new Dictionary<string, string> {
            { "word", request.Word },
            { "exclude", request.Exclude },
            { "include", request.Include }
        });
    }

    private static WordleAssistRequest ExtractQueryParameters(HttpRequest req)
    {
        var word = req.Query["word"].ToString().ToLowerInvariant();
        var exclude = req.Query["exclude"].ToString().ToLowerInvariant();
        var include = req.Query["include"].ToString().ToLowerInvariant();

        return new WordleAssistRequest(word, exclude, include);
    }

    private static bool TryValidate(WordleAssistRequest request, out string error)
    {
        if (string.IsNullOrEmpty(request.Word))
        {
            error = "Query parameter 'word' is required.";
            return false;
        }
        if (!IsAllowed(request.Word, allowUnderscore: true))
        {
            error = "Query parameter 'word' may only contain letters and '_'.";
            return false;
        }
        if (!IsAllowed(request.Exclude))
        {
            error = "Query parameter 'exclude' may only contain letters.";
            return false;
        }
        if (!IsAllowed(request.Include, allowUnderscore: true, allowComma: true))
        {
            error = "Query parameter 'include' may only contain letters, '_', and ','.";
            return false;
        }
        error = "";
        return true;
    }

    private static bool IsAllowed(string s, bool allowUnderscore = false, bool allowComma = false)
    {
        foreach (var c in s)
        {
            if (c is >= 'a' and <= 'z') continue;
            if (allowUnderscore && c == '_') continue;
            if (allowComma && c == ',') continue;
            return false;
        }
        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Request Received - {@Request}")]
    private static partial void LogRequestReceived(ILogger logger, WordleAssistRequest request);
}
