namespace TagCloudGenerator;

public class FrequencyWordAnalyzer : IWordAnalyzer
{
    public Result<Dictionary<string, int>> Analyze(IEnumerable<string> words)
    {
        return Result.Of(() =>
        {
            var frequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var word in words)
            {
                frequencies.TryGetValue(word, out var count);
                frequencies[word] = count + 1;
            }
            
            return frequencies;
        }).ReplaceError(err => $"Error during word frequency analysis: {err}");
    }
}