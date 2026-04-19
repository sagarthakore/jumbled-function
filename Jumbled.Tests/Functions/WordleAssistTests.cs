using Jumbled.Functions;
using Jumbled.Models;
using Jumbled.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace Jumbled.Tests.Functions;

public class WordleAssistTests
{
    private static (WordleAssist sut, IWordleAssistService service) CreateSut()
    {
        var logger = Substitute.For<ILogger<WordleAssist>>();
        var service = Substitute.For<IWordleAssistService>();
        var telemetry = new TelemetryClient(new TelemetryConfiguration());
        return (new WordleAssist(logger, service, telemetry), service);
    }

    private static HttpRequest BuildRequest(params (string key, string value)[] query)
    {
        var context = new DefaultHttpContext();
        var dict = query.ToDictionary(q => q.key, q => new StringValues(q.value));
        context.Request.Query = new QueryCollection(dict);
        return context.Request;
    }

    [Fact]
    public void Run_AllParametersPresent_LowercasesAndPassesToService()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string> { "alpha" });

        var result = sut.Run(BuildRequest(
            ("word", "ABC"),
            ("exclude", "XY"),
            ("include", "_A_")));

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "abc" && r.Exclude == "xy" && r.Include == "_a_"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(new List<string> { "alpha" }, ok.Value);
    }

    [Fact]
    public void Run_OnlyWordProvided_ExcludeAndIncludeDefaultToEmpty()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string>());

        sut.Run(BuildRequest(("word", "abc")));

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "abc" && r.Exclude == "" && r.Include == ""));
    }

    [Fact]
    public void Run_NoQueryParameters_AllFieldsEmpty()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string>());

        sut.Run(BuildRequest());

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "" && r.Exclude == "" && r.Include == ""));
    }

    [Fact]
    public void Run_ServiceReturnsList_WrappedInOkObjectResult()
    {
        var (sut, service) = CreateSut();
        var expected = new List<string> { "one", "two", "three" };
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(expected);

        var result = sut.Run(BuildRequest(("word", "_r___")));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }
}
