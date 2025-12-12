namespace TagCloudGenerator;

public interface IWordPreprocessor
{
    IEnumerable<string> Process(IEnumerable<string> words);
}