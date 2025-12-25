using System.Drawing;

namespace TagCloudGenerator;

public class TagCloudVisualizationConfig
{
    public string OutputFileName { get; set; }
    public Size? ImageSize { get; set; }
    public Color BackgroundColor { get; set; }

    public TagCloudVisualizationConfig(
        string outputFileName,
        Size? imageSize = null,
        Color? backgroundColor = null)
    {
        OutputFileName = outputFileName;
        ImageSize = imageSize;
        BackgroundColor = backgroundColor ?? Color.White;
    }
}