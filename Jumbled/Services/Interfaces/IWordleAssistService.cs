namespace Jumbled.Services.Interfaces;

public interface IWordleAssistService
{
    HashSet<string> GetDictionaryWords(string jumbledWord);
    List<string> GetWordGuess(string guess, string exclude, string include);
}