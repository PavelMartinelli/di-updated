using System.Text.RegularExpressions;
using Nestor;
using Nestor.Models;

namespace TagCloudGenerator;

public class WordPreprocessor : IWordPreprocessor
{
    private readonly HashSet<string> _customStopWords;
    private readonly bool _toLower;
    private readonly NestorMorph _nMorph;
    
    private readonly HashSet<Pos> _functionalPartsOfSpeech =
    [
        Pos.Preposition, // предлог
        Pos.Conjunction, // союз
        Pos.Particle, // частица
        Pos.Interjection, // междометие
        Pos.Parenthesis, // вводное слово
        Pos.Pronoun // местоимения 
    ];

    public WordPreprocessor(IEnumerable<string> customStopWords,
        bool toLower = true)
    {
        _customStopWords = new HashSet<string>(
            customStopWords ?? [],
            StringComparer.OrdinalIgnoreCase);

        _toLower = toLower;

        _nMorph = InitializeNestorSilently();
    }
    
    private NestorMorph InitializeNestorSilently()
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
    
        try
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        
            return new NestorMorph();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    public IEnumerable<string> Process(IEnumerable<string> lines)
    {
        var allWords = ExtractWords(lines);

        return !allWords.Any() 
            ? [] 
            : ProcessWords(allWords);
    }

    private List<string> ExtractWords(IEnumerable<string> lines)
    {
        var wordRegex = new Regex(@"\p{L}+", RegexOptions.Compiled);
        var allWords = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var matches = wordRegex.Matches(line);
            foreach (Match match in matches)
            {
                var word = NormalizeWord(match.Value);
                if (!string.IsNullOrWhiteSpace(word))
                    allWords.Add(word);
            }
        }

        return allWords;
    }

    private string NormalizeWord(string rawWord)
    {
        if (string.IsNullOrWhiteSpace(rawWord))
            return string.Empty;

        return _toLower ? rawWord.ToLowerInvariant() : rawWord;
    }

    private IEnumerable<string> ProcessWords(
        List<string> words)
    {
        var processedWords = new List<string>();

        foreach (var word in words)
        {
            var processedWord = ProcessWord(word);
            if (!string.IsNullOrWhiteSpace(processedWord))
                processedWords.Add(processedWord);
        }

        return processedWords;
    }

    private string ProcessWord(string word)
    {
        if (_customStopWords.Contains(word))
            return string.Empty;
        
        return _customStopWords.Any() 
            ? ProcessWordWithCustomStopWords(word) 
            : ProcessWordWithMorphology(word);
    }

    private string ProcessWordWithCustomStopWords(string word)
    {
        var lemma = TryGetLemma(word);
        return !string.IsNullOrWhiteSpace(lemma) ? lemma : word;
    }

    private string ProcessWordWithMorphology(string word)
    {
        var wordInfos = GetWordInfo(word);

        if (!wordInfos.Any() || IsFunctionalWord(wordInfos))
            return string.Empty;

        return GetLemma(wordInfos, word);
    }

    private Word[] GetWordInfo(string word)
    {
        try
        {
            return _nMorph.WordInfo(word);
        }
        catch
        {
            return [];
        }
    }

    private string TryGetLemma(string word)
    {
        try
        {
            var wordInfos = _nMorph.WordInfo(word);
            if (wordInfos.Any())
                return GetLemma(wordInfos, word);
        }
        catch
        {
            // Игнорируем ошибки при получении леммы
        }

        return null;
    }

    private bool IsFunctionalWord(Word[] wordInfos)
    {
        if (!wordInfos.Any())
            return false;

        var word = wordInfos[0];

        var pos = word.Tag.Pos;

        return _functionalPartsOfSpeech.Contains(pos);
    }

    private string GetLemma(Word[] wordInfos, string originalWord)
    {
        if (!wordInfos.Any())
            return _toLower ? originalWord.ToLowerInvariant() : originalWord;
        var word = wordInfos[0];

        var lemma = word.Lemma?.Word;

        if (string.IsNullOrWhiteSpace(lemma))
            return _toLower ? originalWord.ToLowerInvariant() : originalWord;

        return _toLower ? lemma.ToLowerInvariant() : lemma;
    }
}