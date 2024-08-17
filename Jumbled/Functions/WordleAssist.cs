using Jumbled.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jumbled.Functions;

public class WordleAssist(ILogger<WordleAssist> logger, IWordleAssistService wordleAssist)
{
    private readonly ILogger<WordleAssist> _logger = logger;
    private readonly IWordleAssistService _wordleAssist = wordleAssist;

    [Function("WordleAssist")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Request Received");

        string word = req.Query["word"].ToString();
        string exclude = req.Query["exclude"].ToString();
        string include = req.Query["include"].ToString();

        return new OkObjectResult(_wordleAssist.GetWordGuess(word, exclude, include));
    }
}