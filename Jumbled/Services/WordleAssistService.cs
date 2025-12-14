using Jumbled.Models;
using Jumbled.Services.Interfaces;
using System.Reflection;

namespace Jumbled.Services;

public class WordleAssistService : IWordleAssistService
{
    private readonly Dictionary<string, HashSet<string>> _dictionary;
    private readonly List<string> _words;

    public WordleAssistService()
    {
        var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory!));

        _words = [.. File.ReadAllLines(rootDirectory + "/Resources/words_en.txt")];
        _dictionary = CreateDictionary();
    }

    public HashSet<string> GetDictionaryWords(string jumbledWord)
    {
        var jumbledWordKey = GenerateWordKey(jumbledWord.ToLower());
        return _dictionary.TryGetValue(jumbledWordKey, out HashSet<string>? value) ? value : [];
    }

    public List<string> GetWordGuess(WordleAssistRequest request)
    {
        if (request.Word.Length == 0) return [];

        var filteredWords = _words.Where(word => word.Length == request.Word.Length).ToHashSet();

        if (!string.IsNullOrEmpty(request.Exclude))
        {
            filteredWords = FilterByExclude(filteredWords, request.Exclude);
        }

        if (!string.IsNullOrEmpty(request.Include) && request.Include.Length == request.Word.Length)
        {
            filteredWords = FilterByInclude(filteredWords, request.Include);
        }

        return FilterByGuessPattern(filteredWords, request.Word);
    }

    private static HashSet<string> FilterByExclude(HashSet<string> words, string exclude)
    {
        var result = words.Where(word => !exclude.Any(ch => word.Contains(ch))).ToHashSet();
        return result;
    }

    private static HashSet<string> FilterByInclude(HashSet<string> words, string include)
    {
        var result = words.Where(word =>
            !Enumerable.Range(0, include.Length).Any(i =>
                include[i] != '_' && (!word.Contains(include[i]) || include[i] == word[i])
            )
        ).ToHashSet();
        return result;
    }

    private static List<string> FilterByGuessPattern(HashSet<string> words, string guess)
    {
        var result = new List<string>();
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
            if (match) result.Add(word);
        }
        return result;
    }

    private Dictionary<string, HashSet<string>> CreateDictionary()
    {
        Dictionary<string, HashSet<string>> dict = [];
        foreach (var word in _words)
        {
            var wordKey = GenerateWordKey(word);
            if (!dict.TryGetValue(wordKey, out HashSet<string>? wordList))
            {
                wordList = [];
                dict[wordKey] = wordList;
            }
            wordList.Add(word);
        }
        return dict;
    }

    private static string GenerateWordKey(string inputString)
    {
        var chars = inputString.ToCharArray();
        Array.Sort(chars);
        return string.Concat(chars);
    }
}