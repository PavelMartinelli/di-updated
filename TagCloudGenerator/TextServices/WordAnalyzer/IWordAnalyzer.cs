namespace TagCloudGenerator;

public interface IWordAnalyzer
{
    Dictionary<string, int> Analyze(IEnumerable<string> words);
}