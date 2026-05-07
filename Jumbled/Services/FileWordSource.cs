using Jumbled.Services.Interfaces;
using System.Reflection;

namespace Jumbled.Services;

public sealed class FileWordSource : IWordSource
{
    private readonly Lazy<IReadOnlyList<string>> _words = new(LoadWords);

    public IReadOnlyList<string> GetWords() => _words.Value;

    private static IReadOnlyList<string> LoadWords()
    {
        var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Unable to determine assembly location.");
        var path = Path.Combine(binDirectory, "Resources", "words_en.txt");
        return File.ReadAllLines(path);
    }
}
