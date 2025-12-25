using System.Drawing;

namespace TagCloudGenerator;

public class TagCloudProviderConfig
{
    public Point Center { get; set; }
    public Dictionary<string, int> WordsWithFrequencies { get; set; }
    public string FontFamily { get; set; }
    public int MinFontSize { get; set; }
    public int MaxFontSize { get; set; }

    public TagCloudProviderConfig(
        Point center, 
        Dictionary<string, int> wordsWithFrequencies,
        string fontFamily = "Arial",
        int minFontSize = 10,
        int maxFontSize = 50)
    {
        Center = center;
        WordsWithFrequencies = wordsWithFrequencies ?? throw new ArgumentNullException(nameof(wordsWithFrequencies));
        FontFamily = fontFamily;
        MinFontSize = minFontSize;
        MaxFontSize = maxFontSize;
    }
}