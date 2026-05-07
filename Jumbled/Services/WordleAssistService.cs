using Jumbled.Models;
using Jumbled.Services.Interfaces;
using System.Collections.Frozen;

namespace Jumbled.Services;

public class WordleAssistService : IWordleAssistService
{
    private readonly FrozenDictionary<int, string[]> _wordsByLength;

    public WordleAssistService(IWordSource wordSource)
    {
        _wordsByLength = wordSource.GetWords()
            .GroupBy(w => w.Length)
            .ToFrozenDictionary(g => g.Key, g => g.ToArray());
    }

    public List<string> GetWordGuess(WordleAssistRequest request)
    {
        if (string.IsNullOrEmpty(request.Word)) return [];

        if (!_wordsByLength.TryGetValue(request.Word.Length, out var candidates))
        {
            return [];
        }

        IEnumerable<string> filtered = candidates;

        if (!string.IsNullOrEmpty(request.Exclude))
        {
            filtered = FilterByExclude(filtered, request.Exclude);
        }

        if (!string.IsNullOrEmpty(request.Include))
        {
            var includePatterns = request.Include.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pattern in includePatterns)
            {
                if (pattern.Length == request.Word.Length)
                {
                    filtered = FilterByInclude(filtered, pattern);
                }
            }
        }

        return FilterByGuessPattern(filtered, request.Word).Order().ToList();
    }

    private static IEnumerable<string> FilterByExclude(IEnumerable<string> words, string exclude)
    {
        var bad = exclude.ToHashSet();
        return words.Where(word => !word.Any(bad.Contains));
    }

    private static IEnumerable<string> FilterByInclude(IEnumerable<string> words, string include)
    {
        return words.Where(word =>
            !Enumerable.Range(0, include.Length).Any(i =>
                include[i] != '_' && (!word.Contains(include[i]) || include[i] == word[i])
            )
        );
    }

    private static IEnumerable<string> FilterByGuessPattern(IEnumerable<string> words, string guess)
    {
        foreach (var word in words)
        {
            bool match = true;
            for (var i = 0; i < guess.Length; i++)
            {
                if (guess[i] != '_' && guess[i] != word[i])
                {
                    match = false;
                    break;
                }
            }
            if (match) yield return word;
        }
    }
}
