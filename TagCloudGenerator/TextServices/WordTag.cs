using System.Drawing;

namespace TagCloudGenerator;

public class WordTag
{
    public string Text { get; }
    public int Frequency { get; }
    public  Font Font { get; }
    public Color Color { get; }
    public Rectangle Rectangle { get; set; }

    public WordTag(string text, int frequency, Font font, Color color)
    {
        Text = text;
        Frequency = frequency;
        Font = font;
        Color = color;
    }

    public override string ToString() => $"Word: '{Text}' (freq: {Frequency})";
}