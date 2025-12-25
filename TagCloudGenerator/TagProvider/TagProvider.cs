namespace TagCloudGenerator;

using System.Drawing;

public class TagProvider : ITagProvider
{
    public Result<IEnumerable<WordTag>> GetTags(
        TagCloudProviderConfig config,
        IFontSizeCalculator fontSizeCalculator,
        IColorScheme colorProvider)
    {
        return Result.Of(() =>
        {
            var sortedWords = config.WordsWithFrequencies
                .OrderByDescending(pair => pair.Value)
                .ToList();

            var totalWords = sortedWords.Count;
            var result = new List<WordTag>();

            for (var i = 0; i < sortedWords.Count; i++)
            {
                var (word, frequency) = sortedWords[i];
                
                var fontSize = fontSizeCalculator.CalculateFontSize(
                    frequency, 
                    config.MinFontSize, 
                    config.MaxFontSize);
                
                var font = new Font(config.FontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                
                var color = colorProvider.GetColorForWord(
                    new WordTag(word, frequency, font, Color.Black),
                    i, 
                    totalWords);

                result.Add(new WordTag(word, frequency, font, color));
            }

            return result.AsEnumerable();
        }).ReplaceError(err => $"Error during tag generation: {err}");
    }
}