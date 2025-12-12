namespace TagCloudGenerator;

using System.Drawing;

public class GraphicsTextMeasurer : ITextMeasurer, IDisposable
{
    private readonly Bitmap _fakeBitmap;
    private readonly Graphics _graphics;

    public GraphicsTextMeasurer()
    {
        _fakeBitmap = new Bitmap(1, 1);
        _graphics = Graphics.FromImage(_fakeBitmap);
        _graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
    }

    public Size MeasureString(string text, Font font)
    {
        var sizeF = _graphics.MeasureString(text, font);
        return new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
    }

    public void Dispose()
    {
        _graphics.Dispose();
        _fakeBitmap.Dispose();
    }
}