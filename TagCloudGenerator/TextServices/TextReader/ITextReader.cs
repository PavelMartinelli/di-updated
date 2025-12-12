namespace TagCloudGenerator;

public interface ITextReader
{
    bool CanRead(string filePath);
    IEnumerable<string> ReadLines(string filePath);
}