using System.Drawing;

namespace TagCloudGenerator;

public interface ITextMeasurer
{
    Size MeasureString(string text, Font font);
}