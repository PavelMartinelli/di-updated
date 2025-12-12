using System.Drawing;

namespace TagCloudGenerator;

public class RandomColorScheme : IColorScheme
{
    private readonly Random _random;

    private readonly List<Color> _palette = [
        Color.FromArgb(255, 182, 193),
        Color.FromArgb(173, 216, 230),
        Color.FromArgb(144, 238, 144),
        Color.FromArgb(255, 222, 173),
        Color.FromArgb(221, 160, 221),
        Color.FromArgb(240, 230, 140),
        Color.FromArgb(176, 224, 230)
    ];

    public RandomColorScheme(Random? random = null)
    {
        _random = random ?? new Random();
    }

    public Color GetColorForWord(WordTag word, int index, int total)
    {
        return _palette[_random.Next(_palette.Count)];
    }
}