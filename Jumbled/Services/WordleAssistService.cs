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

    public List<string> GetWordGuess(string guess, string exclude, string include)
    {
        if (guess.Length == 0) return [];

        var result = new List<string>();
        HashSet<string> filteredWords = new(_words.Where(word => word.Length == guess.Length));

        if (!string.IsNullOrEmpty(exclude))
        {
            foreach (var word in _words.Where(word => word.Length == guess.Length))
            {
                var candidate = false;
                for (var i = 0; i < exclude.Length; i++)
                {
                    if (word.Contains(exclude[i]))
                    {
                        candidate = true;
                        break;
                    }
                }

                if (candidate) filteredWords.Remove(word);
            }
        }

        if (!string.IsNullOrEmpty(include) && include.Length == guess.Length)
        {
            foreach (var word in _words.Where(word => word.Length == guess.Length))
            {
                var candidate = false;
                for (var i = 0; i < include.Length; i++)
                {
                    if (include[i] != '_' && (!word.Contains(include[i]) || include[i] == word[i]))
                    {
                        candidate = true;
                        break;
                    }
                }

                if (candidate) filteredWords.Remove(word);
            }
        }

        foreach (var word in filteredWords)
        {
            var candidate = true;
            for (var i = 0; i < guess.Length; i++)
            {
                if (guess[i] != '_' && guess[i] != word[i])
                {
                    candidate = false;
                    break;
                }
            }

            if (candidate) result.Add(word);
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