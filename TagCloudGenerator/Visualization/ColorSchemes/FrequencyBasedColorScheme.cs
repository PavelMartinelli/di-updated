using System.Drawing;

namespace TagCloudGenerator;

public class FrequencyBasedColorScheme : IColorScheme
{
    private readonly Color _lowFrequencyColor;
    private readonly Color _highFrequencyColor;

    public FrequencyBasedColorScheme(Color lowFrequencyColor, Color highFrequencyColor)
    {
        _lowFrequencyColor = lowFrequencyColor;
        _highFrequencyColor = highFrequencyColor;
    }

    public  Color GetColorForWord(WordTag word, int index, int total)
    {
        var ratio = (float)word.Frequency / (word.Frequency + 10);
        
        var r = (int)(_lowFrequencyColor.R + (_highFrequencyColor.R - _lowFrequencyColor.R) * ratio);
        var g = (int)(_lowFrequencyColor.G + (_highFrequencyColor.G - _lowFrequencyColor.G) * ratio);
        var b = (int)(_lowFrequencyColor.B + (_highFrequencyColor.B - _lowFrequencyColor.B) * ratio);
        
        return Color.FromArgb(r, g, b);
    }
}