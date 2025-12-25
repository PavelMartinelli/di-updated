using System.Drawing;

namespace TagCloudGenerator;

public class AppSettings
{
    public static Color DefaultBackgroundColor { get; } = Color.Indigo;
    public string InputFile { get; set; } = string.Empty;
    public string OutputFile { get; set; } = "tagcloud.png";
    public string FontFamily { get; set; } = "Arial";
    public int MinFontSize { get; set; } = 20;
    public int MaxFontSize { get; set; } = 60;
    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 900;
    public Color? BackgroundColor { get; set; } = DefaultBackgroundColor;
    public string ColorScheme { get; set; } = "Random";
    public string Algorithm { get; set; } = "Spiral";
    public Point Center { get; set; } = new Point(400, 300);
    
    public List<string> StopWords { get; set; } = new();
    public bool ToLowerCase { get; set; } = true;
    
    public Result<None> Validate()
    {
        if (string.IsNullOrWhiteSpace(InputFile))
            return Result.Fail<None>("Input file is required");
        
        if (MinFontSize <= 0 || MaxFontSize <= 0)
            return Result.Fail<None>("Font sizes must be positive");
        
        if (MinFontSize > MaxFontSize)
            return Result.Fail<None>("Minimum font size must be less than or equal to maximum font size");
        
        if (Width <= 0 || Height <= 0)
            return Result.Fail<None>("Image dimensions must be positive");
        
        return Result.Ok();
    }
}