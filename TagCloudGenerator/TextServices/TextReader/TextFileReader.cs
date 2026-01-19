namespace TagCloudGenerator;

public class TextFileReader : ITextReader
{
    public Result<IEnumerable<string>> ReadLines(string filePath)
    {
        return Result.Of(() =>
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return File.ReadLines(filePath).AsEnumerable();
        }).ReplaceError(err => $"Cannot read file '{filePath}': {err}");
    }
}