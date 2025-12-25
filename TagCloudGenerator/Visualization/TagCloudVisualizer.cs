using System.Drawing;

namespace TagCloudGenerator;

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

    public Result<string> SaveVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config)
    {
        return Result.Ok(tags.ToList())
            .Then(tagList => CalculateOptimalImageSize(tagList))
            .Then(imageSize => CreateBitmap(imageSize))
            .Then(bitmap => DrawTagsOnBitmap(bitmap, tags, config))
            .Then(bitmap => _imageSaver.SaveBitmap(bitmap, config.OutputFileName))
            .ReplaceError(err => $"Error during visualization creation or saving: {err}");
    }

    private Result<Size> CalculateOptimalImageSize(List<WordTag> tags)
    {
        if (!tags.Any())
            return Result.Ok(new Size(DefaultWidth, DefaultHeight));

        var minX = tags.Min(t => t.Rectangle.Left);
        var maxX = tags.Max(t => t.Rectangle.Right);
        var minY = tags.Min(t => t.Rectangle.Top);
        var maxY = tags.Max(t => t.Rectangle.Bottom);

        var width = Math.Max(DefaultWidth, (maxX - minX) + Padding);
        var height = Math.Max(DefaultHeight, (maxY - minY) + Padding);

        return Result.Ok(new Size(width, height));
    }

    private Result<Bitmap> CreateBitmap(Size size)
    {
        return Result.Of(() => new Bitmap(size.Width, size.Height));
    }

    private Result<Bitmap> DrawTagsOnBitmap(Bitmap bitmap, IEnumerable<WordTag> tags, TagCloudVisualizationConfig config)
    {
        return Result.Of(() =>
        {
            using var graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(config.BackgroundColor);
            
            foreach (var tag in tags)
            {
                using var textBrush = new SolidBrush(tag.Color);
                graphics.DrawString(
                    tag.Text,
                    tag.Font,
                    textBrush,
                    tag.Rectangle.Location);
            }
            
            return bitmap;
        });
    }
}