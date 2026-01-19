namespace TagCloudGenerator;

public interface IWordAnalyzer
{
    Result<Dictionary<string, int>> Analyze(IEnumerable<string> words);
}