namespace TagCloudGenerator;

public class WordPreprocessor : IWordPreprocessor
{
    private readonly HashSet<string> _stopWords;
    private readonly bool _toLower;
    private static readonly string[] DefaultStopWords =
    [
        "a", "an", "the", "and", "or", "but", "in", "on", "at", "to",
        "of", "for", "with", "by", "as", "is", "was", "were", "be",
        "been", "have", "has", "had", "do", "does", "did", "will",
        "would", "should", "could", "may", "might", "must", "can"
    ];

    public WordPreprocessor(IEnumerable<string> stopWords, bool toLower = true)
    {
        var wordsToUse = stopWords as ICollection<string> ?? stopWords.ToList();
        _stopWords = wordsToUse.Any() 
            ? new HashSet<string>(wordsToUse, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(DefaultStopWords, StringComparer.OrdinalIgnoreCase);
        
        _toLower = toLower;
    }

    public IEnumerable<string> Process(IEnumerable<string> words)
    {
        foreach (var word in words)
        {
            var processed = word;
            
            if (_toLower)
                processed = processed.ToLowerInvariant();
                
            if (string.IsNullOrWhiteSpace(processed) || _stopWords.Contains(processed))
                continue;
                
            yield return processed;
        }
    }
}