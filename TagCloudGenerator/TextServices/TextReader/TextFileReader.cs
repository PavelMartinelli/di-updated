namespace TagCloudGenerator;

public class TextFileReader : ITextReader
{
    public bool CanRead(string filePath)
    {
        return File.Exists(filePath);
    }

    public IEnumerable<string> ReadLines(string filePath)
    {
        using var reader = new StreamReader(filePath);
        while (reader.ReadLine() is { } line)
            yield return line;
    }
}