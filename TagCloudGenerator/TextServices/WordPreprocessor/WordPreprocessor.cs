using System.Text.RegularExpressions;
using Nestor;
using Nestor.Models;

namespace TagCloudGenerator;

public class WordPreprocessor : IWordPreprocessor
{
    private static readonly Regex WordRegex = new Regex(@"\p{L}+", RegexOptions.Compiled);
    
    private readonly HashSet<string> _customStopWords;
    private readonly bool _toLower;
    private readonly Lazy<Result<NestorMorph>> _nMorph;

    private readonly HashSet<Pos> _functionalPartsOfSpeech =
    [
        Pos.Preposition,
        Pos.Conjunction,
        Pos.Particle,
        Pos.Interjection,
        Pos.Parenthesis,
        Pos.Pronoun
    ];

    public WordPreprocessor(IEnumerable<string> customStopWords, bool toLower = true)
    {
        _customStopWords = new HashSet<string>(
            customStopWords ?? [],
            StringComparer.OrdinalIgnoreCase);

        _toLower = toLower;
        _nMorph = new Lazy<Result<NestorMorph>>(() => InitializeNestor());
    }

    private Result<NestorMorph> InitializeNestor()
    {
        return Result.Of(() =>
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
            
            var morph = new NestorMorph();
            
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            
            return morph;
        }).ReplaceError(err => 
            "Failed to initialize morphological analyzer. " +
            "Make sure Nestor library is properly installed. " +
            $"Error: {err}");
    }

    public Result<IEnumerable<string>> Process(IEnumerable<string> words)
    {
        return words.AsResult()
            .Then(ws => ExtractWords(ws))
            .Then(allWords => allWords.ToList().AsResult())
            .Then(allWords => ProcessWords(allWords))
            .ReplaceError(err => $"Error during word preprocessing: {err}");
    }

    private Result<List<string>> ExtractWords(IEnumerable<string> lines)
    {
        return lines.AsResult()
            .Then(ls => ls.Where(line => !string.IsNullOrWhiteSpace(line)).AsResult())
            .Then(nonEmptyLines => Result.Of(() =>
            {
                return nonEmptyLines
                    .SelectMany(line => WordRegex.Matches(line).Cast<Match>())
                    .Select(match => NormalizeWord(match.Value))
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .ToList();
            }));
    }

    private string NormalizeWord(string rawWord)
    {
        if (string.IsNullOrWhiteSpace(rawWord))
            return string.Empty;

        return _toLower ? rawWord.ToLowerInvariant() : rawWord;
    }

    private Result<IEnumerable<string>> ProcessWords(List<string> allWords)
    {
        return allWords.AsResult()
            .Then(words => words
                .Where(word => !_customStopWords.Contains(word))
                .Select(word => ProcessWord(word))
                .ToList()
                .AsResult())
            .Then(processedWordResults => processedWordResults
                .Where(result => result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value))
                .Select(result => result.Value)
                .ToList()
                .AsEnumerable()
                .AsResult());
    }

    private Result<string> ProcessWord(string word)
    {
        return _customStopWords.Any()
            ? ProcessWordWithCustomStopWords(word)
            : ProcessWordWithMorphology(word);
    }

    private Result<string> ProcessWordWithCustomStopWords(string word)
    {
        return _nMorph.Value
            .Then(nMorph => TryGetLemmaWithNestor(nMorph, word))
            .Then(lemma => string.IsNullOrWhiteSpace(lemma) 
                ? Result.Ok(word) 
                : Result.Ok(_toLower ? lemma.ToLowerInvariant() : lemma))
            .OnFail(_ => Result.Ok(word));
    }

    private Result<string> TryGetLemmaWithNestor(NestorMorph nMorph, string word)
    {
        return Result.Of(() =>
        {
            var wordInfos = nMorph.WordInfo(word);
            if (!wordInfos.Any())
                return null;

            var lemma = wordInfos[0].Lemma?.Word;
            return string.IsNullOrWhiteSpace(lemma) ? null : lemma;
        }).OnFail(_ => Result.Ok(word));
    }

    private Result<string> ProcessWordWithMorphology(string word)
    {
        return _nMorph.Value
            .Then(nMorph => Result.Of(() => nMorph.WordInfo(word)))
            .Then(wordInfos => wordInfos.Any()
                ? Result.Ok(wordInfos)
                : Result.Fail<Word[]>("No word info"))
            .Then(wordInfos => IsFunctionalWord(wordInfos)
                ? Result.Ok(string.Empty)
                : GetWordLemma(wordInfos, word))
            .ReplaceError(_ => string.Empty);
    }

    private bool IsFunctionalWord(Word[] wordInfos)
    {
        return wordInfos.Any() && _functionalPartsOfSpeech.Contains(wordInfos[0].Tag.Pos);
    }

    private Result<string> GetWordLemma(Word[] wordInfos, string originalWord)
    {
        return wordInfos.Any()
            ? wordInfos[0].Lemma.Word.AsResult()
                .Then(lemma => string.IsNullOrWhiteSpace(lemma)
                    ? originalWord.AsResult()
                    : (_toLower ? lemma.ToLowerInvariant() : lemma).AsResult())
            : originalWord.AsResult();
    }
}