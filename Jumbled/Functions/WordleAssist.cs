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

        LogRequestReceived(logger, request);
        TrackTelemetryEvent(request);

        return new OkObjectResult(wordleAssist.GetWordGuess(request));
    }

    private static WordleAssistRequest ExtractQueryParameters(HttpRequest req)
    {
        var word = req.Query["word"].ToString().ToLowerInvariant();
        var exclude = req.Query["exclude"].ToString().ToLowerInvariant();
        var include = req.Query["include"].ToString().ToLowerInvariant();

        return new WordleAssistRequest(word, exclude, include);
    }

    private void TrackTelemetryEvent(WordleAssistRequest request)
    {
        telemetryClient.TrackEvent("Request Received", new Dictionary<string, string> {
            { "word", request.Word },
            { "exclude", request.Exclude },
            { "include", request.Include }
        });
    }

    // NOTE: [ExcludeFromCodeCoverage] is intentionally NOT applied here. Doing so triggers
    // Coverlet 6.0.4's AnalyzeCompileGeneratedTypesForExcludedMethod path, which reads all
    // custom attributes on the method (including [LoggerMessage]) and fails to resolve
    // Microsoft.Extensions.Logging.Abstractions (a shared-framework assembly not copied to
    // the test output), causing the whole module to be skipped during instrumentation.
    // The source-generated body lives in LoggerMessage.g.cs and is excluded via
    // <ExcludeByFile>**/*.g.cs</ExcludeByFile> in coverlet.runsettings. The partial
    // declaration below has no body, so it contributes no instrumentable IL.
    [LoggerMessage(Level = LogLevel.Information, Message = "Request Received - {@Request}")]
    private static partial void LogRequestReceived(ILogger logger, WordleAssistRequest request);
}