using Jumbled.Models;
using Jumbled.Services;

namespace Jumbled.Tests.Services;

public sealed class WordleAssistServiceFixture
{
    public WordleAssistService Service { get; } = new(new FileWordSource());
}

public class WordleAssistServiceTests(WordleAssistServiceFixture fixture) : IClassFixture<WordleAssistServiceFixture>
{
    private readonly WordleAssistService _wordleAssistService = fixture.Service;

    [Fact]
    public void GetWordGuessWord_WordsExist_GetWords()
    {
        var request = new WordleAssistRequest("f______rk");
        var result = _wordleAssistService.GetWordGuess(request);
        List<string> expected =
        [
            "fancywork",
            "fieldwork",
            "framework",
            "frostwork"
        ];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordGuessWordExcludeLetters_WordsExist_GetWords()
    {
        var request = new WordleAssistRequest("_rick", "tb");
        var result = _wordleAssistService.GetWordGuess(request);
        List<string> expected =
        [
            "crick",
            "prick"
        ];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordGuessWordIncludeLetters_WordsExist_GetWords()
    {
        var request = new WordleAssistRequest("_ric_", "", "____b");
        var result = _wordleAssistService.GetWordGuess(request);
        List<string> expected = ["brick"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordGuessWordIncludeExcludeLetters_WordsExist_GetWords()
    {
        var request = new WordleAssistRequest("_o___", "ad", "b__r_");
        var result = _wordleAssistService.GetWordGuess(request);
        List<string> expected =
        [
            "robes",
            "robin",
            "roble",
            "robot",
            "sober"
        ];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordGuess_WordsDontExist_GetEmptyArray()
    {
        var request = new WordleAssistRequest("kl__fd__h_la");
        var result = _wordleAssistService.GetWordGuess(request);
        Assert.Empty(result);
    }
}
