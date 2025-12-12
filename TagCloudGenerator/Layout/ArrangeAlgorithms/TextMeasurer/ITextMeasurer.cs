namespace TagCloudGenerator;

using System.Drawing;

public interface ITextMeasurer
{
    Size MeasureString(string text, Font font);
}