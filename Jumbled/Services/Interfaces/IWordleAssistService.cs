using Jumbled.Models;

namespace Jumbled.Services.Interfaces;

public interface IWordleAssistService
{
    HashSet<string> GetDictionaryWords(string jumbledWord);
    List<string> GetWordGuess(WordleAssistRequest request);
}