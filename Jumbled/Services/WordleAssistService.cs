using Jumbled.Services.Interfaces;
using System.Reflection;

namespace Jumbled.Services;

public class WordleAssistService : IWordleAssistService
{
    private readonly Dictionary<string, HashSet<string>> dictionary;
    private readonly List<string> words;

    public WordleAssistService()
    {
        var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory!));

        words = [.. File.ReadAllLines(rootDirectory + "/Resources/words_en.txt")];
        dictionary = CreateDictionary();
    }

    public HashSet<string> GetDictionaryWords(string jumbledWord)
    {
        string jumbledWordKey = GenerateWordKey(jumbledWord.ToLower());
        return dictionary.TryGetValue(jumbledWordKey, out HashSet<string>? value) ? value : [];
    }

    public List<string> GetWordGuess(string guess, string exclude, string include)
    {
        if (guess.Length == 0) return [];

        var result = new List<string>();
        HashSet<string> filteredWords = new(words.Where(word => word.Length == guess.Length));

        if (!string.IsNullOrEmpty(exclude))
        {
            foreach (string word in words.Where(word => word.Length == guess.Length))
            {
                bool candidate = false;
                for (int i = 0; i < exclude.Length; i++)
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
            foreach (string word in words.Where(word => word.Length == guess.Length))
            {
                bool candidate = false;
                for (int i = 0; i < include.Length; i++)
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

        foreach (string word in filteredWords)
        {
            bool candidate = true;
            for (int i = 0; i < guess.Length; i++)
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
        foreach (string word in words)
        {
            string wordKey = GenerateWordKey(word);
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
        char[] chars = inputString.ToCharArray();
        Array.Sort(chars);
        return string.Concat(chars);
    }
}