using Jumbled.Services;

namespace Jumbled.Tests;

public class WordleAssistServiceTests
{
    private readonly WordleAssistService _wordleAssistService;

    public WordleAssistServiceTests()
    {
        _wordleAssistService = new WordleAssistService();
    }

    [Theory]
    [InlineData("danger")]
    public void GetDictionaryWords_WordsExist_GetWords(string value)
    {
        HashSet<string> result = _wordleAssistService.GetDictionaryWords(value);
        HashSet<string> expected =
        [
            "danger",
            "gander",
            "garden",
            "grande",
            "ranged"
        ];

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("f______rk", "", "")]
    public void GetWordGuessWord_WordsExist_GetWords(string value, string exclude, string include)
    {
        List<string> result = _wordleAssistService.GetWordGuess(value, exclude, include);
        List<string> expected =
        [
            "fancywork",
            "fieldwork",
            "framework",
            "frostwork"
        ];

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("_rick", "tb", "")]
    public void GetWordGuessWordExcludeLetters_WordsExist_GetWords(string value, string exclude, string include)
    {
        List<string> result = _wordleAssistService.GetWordGuess(value, exclude, include);
        List<string> expected =
        [
            "crick",
            "prick"
        ];

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("_ric_", "", "____b")]
    public void GetWordGuessWordIncludeLetters_WordsExist_GetWords(string value, string exclude, string include)
    {
        List<string> result = _wordleAssistService.GetWordGuess(value, exclude, include);
        List<string> expected =
        [
            "brick"
        ];

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("_o___", "ad", "b__r_")]
    public void GetWordGuessWordIncludeExcludeLetters_WordsExist_GetWords(string value, string exclude, string include)
    {
        List<string> result = _wordleAssistService.GetWordGuess(value, exclude, include);
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

    [Theory]
    [InlineData("klsjfdkfhfla")]
    public void GetDictionaryWords_WordsDontExist_GetEmptyArray(string value)
    {
        HashSet<string> result = _wordleAssistService.GetDictionaryWords(value);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("kl__fd__h_la", "", "")]
    public void GetWordGuess_WordsDontExist_GetEmptyArray(string value, string exclude, string include)
    {
        List<string> result = _wordleAssistService.GetWordGuess(value, exclude, include);
        Assert.Empty(result);
    }
}