namespace TagCloudGenerator;

public class FrequencyWordAnalyzer : IWordAnalyzer
{
    public Dictionary<string, int> Analyze(IEnumerable<string> words)
    {
        var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var word in words)
        {
            frequencies.TryGetValue(word, out var count);
            frequencies[word] = count + 1;
        }
        
        return frequencies;
    }
}