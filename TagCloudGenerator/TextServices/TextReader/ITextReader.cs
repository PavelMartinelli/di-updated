namespace TagCloudGenerator;

public interface ITextReader
{
    Result<IEnumerable<string>> ReadLines(string filePath);
}