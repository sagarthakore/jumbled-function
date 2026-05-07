namespace Jumbled.Services.Interfaces;

public interface IWordSource
{
    IReadOnlyList<string> GetWords();
}
