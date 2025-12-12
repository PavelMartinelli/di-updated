namespace TagCloudGenerator;

using System.Drawing;
using System.Drawing.Imaging;

public class ImageSaver : IImageSaver
{
    private readonly string _relativeOutputDirectory;

    public ImageSaver(string relativeOutputDirectory = "out")
    {
        _relativeOutputDirectory = relativeOutputDirectory;
    }

    public string SaveBitmap(Bitmap bitmap, string fileName, ImageFormat format = null)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        var actualFormat = format ?? GetFormatFromFileName(fileName);
        
        var projectDir = GetProjectDirectory();
        var outputDir = Path.Combine(projectDir, _relativeOutputDirectory);
        Directory.CreateDirectory(outputDir);
        
        var filePath = Path.Combine(outputDir, fileName);
        bitmap.Save(filePath, actualFormat);
        
        return Path.GetFullPath(filePath);
    }

    private ImageFormat GetFormatFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".png" => ImageFormat.Png,
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            ".tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Png
        };
    }

    private string GetProjectDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var currentDir = baseDir;
        
        while (currentDir != null)
        {
            if (Directory.GetFiles(currentDir, "*.csproj").Any())
                return currentDir;
            
            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }
        
        return baseDir;
    }
}