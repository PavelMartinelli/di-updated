namespace TagCloudGenerator;

using System.Drawing;

public class TagCloudVisualizer : IVisualizer
{
    private const int DefaultWidth = 800;
    private const int DefaultHeight = 600;
    private const int Padding = 200;
    
    private readonly IImageSaver _imageSaver;

    public TagCloudVisualizer(IImageSaver imageSaver)
    {
        _imageSaver = imageSaver ?? throw new ArgumentNullException(nameof(imageSaver));
    }

    public Bitmap CreateVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config)
    {
        var tagList = tags.ToList();
        
        var actualImageSize = config.ImageSize ?? CalculateOptimalImageSize(tagList);
        var bitmap = new Bitmap(actualImageSize.Width, actualImageSize.Height);
        
        using var graphics = Graphics.FromImage(bitmap);
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        graphics.Clear(config.BackgroundColor);
        
        foreach (var tag in tagList)
        {
            using var textBrush = new SolidBrush(tag.Color);
            graphics.DrawString(
                tag.Text,
                tag.Font,
                textBrush,
                tag.Rectangle.Location);
        }
        
        return bitmap;
    }

    public string SaveVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config)
    {
        using var bitmap = CreateVisualization(tags, center, config);
        return _imageSaver.SaveBitmap(bitmap, config.OutputFileName);
    }

    private Size CalculateOptimalImageSize(List<WordTag> tags)
    {
        if (!tags.Any())
            return new Size(DefaultWidth, DefaultHeight);

        var minX = tags.Min(t => t.Rectangle.Left);
        var maxX = tags.Max(t => t.Rectangle.Right);
        var minY = tags.Min(t => t.Rectangle.Top);
        var maxY = tags.Max(t => t.Rectangle.Bottom);

        var width = Math.Max(DefaultWidth, (maxX - minX) + Padding);
        var height = Math.Max(DefaultHeight, (maxY - minY) + Padding);

        return new Size(width, height);
    }
}