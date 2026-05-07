using Jumbled.Models;

namespace Jumbled.Services.Interfaces;

public interface IWordleAssistService
{
    List<string> GetWordGuess(WordleAssistRequest request);
}
