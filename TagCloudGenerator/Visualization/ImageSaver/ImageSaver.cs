using System.Drawing;
using System.Drawing.Imaging;

namespace TagCloudGenerator;

public class ImageSaver : IImageSaver
{
    private readonly string _relativeOutputDirectory;

    public ImageSaver(string relativeOutputDirectory = "out")
    {
        _relativeOutputDirectory = relativeOutputDirectory;
    }

    public Result<string> SaveBitmap(Bitmap bitmap, string fileName, ImageFormat format = null)
    {
        return ValidateParameters(bitmap, fileName)
            .Then(_ => GetImageFormat(fileName))
            .Then(imageFormat => CreateOutputDirectory()
                .Then(_ => SaveImageToFile(bitmap, fileName, imageFormat)))
            .ReplaceError(err => $"Failed to save image '{fileName}': {err}");
    }

    private Result<None> ValidateParameters(Bitmap bitmap, string fileName)
    {
        if (bitmap == null)
            return Result.Fail<None>("Bitmap cannot be null");
        
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Fail<None>("File name cannot be empty");
        
        return Result.Ok();
    }

    private Result<ImageFormat> GetImageFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        var format = extension switch
        {
            ".png" => ImageFormat.Png,
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            ".tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Png
        };
        
        return format.AsResult();
    }
    
    private Result<None> CreateOutputDirectory()
    {
        return Result.OfAction(() =>
        {
            var projectDir = GetProjectDirectory();
            var outputDir = Path.Combine(projectDir, _relativeOutputDirectory);
            Directory.CreateDirectory(outputDir);
        }).ReplaceError(err => $"Failed to create output directory: {err}");
    }

    private Result<string> SaveImageToFile(Bitmap bitmap, string fileName, ImageFormat format)
    {
        return Result.Of(() =>
        {
            var projectDir = GetProjectDirectory();
            var outputDir = Path.Combine(projectDir, _relativeOutputDirectory);
            Directory.CreateDirectory(outputDir);
            
            var filePath = Path.Combine(outputDir, fileName);
            bitmap.Save(filePath, format);
            
            return Path.GetFullPath(filePath);
        });
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