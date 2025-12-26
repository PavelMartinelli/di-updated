namespace TagCloudGenerator;

public interface IWordPreprocessor
{
    Result<IEnumerable<string>> Process(IEnumerable<string> words);
}