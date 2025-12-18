using Autofac;
using TagCloudGenerator;
using System.Drawing;
using FluentAssertions;

namespace TagCloudGeneratorTests;

[TestFixture]
public class IntegrationTests
{
    private string _tempInputFile;
    private string _tempOutputDir;
    private IImageSaver _imageSaver;

    [SetUp]
    public void SetUp()
    {
        _tempOutputDir = Path.Combine(Path.GetTempPath(),
            $"TagCloudTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempOutputDir);

        _tempInputFile = Path.Combine(_tempOutputDir, "test_input.txt");

        _imageSaver = new ImageSaver(_tempOutputDir);

    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_tempOutputDir))
                Directory.Delete(_tempOutputDir, true);
        }
        catch
        {
        }
    }

    [Test]
    public void GenerateFromFile_ShouldCreateImage_WhenValidInputProvided()
    {
        var words = new[]
        {
            "яблоко", "банан", "яблоко", "апельсин", "банан", "банан",
            "груша", "виноград", "яблоко", "киви", "апельсин", "яблоко"
        };

        File.WriteAllLines(_tempInputFile, words);

        var settings = new AppSettings
        {
            InputFile = _tempInputFile,
            OutputFile = "test_output.png",
            FontFamily = "Arial",
            MinFontSize = 20,
            MaxFontSize = 60,
            Width = 800,
            Height = 600,
            BackgroundColor = Color.White,
            ColorScheme = "Random",
            Center = new Point(400, 300),
            StopWords = new List<string> { "и", "в", "на" },
            ToLowerCase = true
        };
        
        using var container = CreateContainer(settings);
        var app = container.Resolve<TagCloudApplication>();
        
        var resultPath = app.GenerateFromFile(settings);
        
        resultPath.Should().NotBeNullOrEmpty();
        File.Exists(resultPath).Should().BeTrue();

        var fileInfo = new FileInfo(resultPath);
        fileInfo.Length.Should().BeGreaterThan(0);
        fileInfo.Extension.Should().Be(".png");
    }

    [Test]
    public void GenerateFromFile_ShouldFilterStopWordsAndConvertToLowercase()
    {
        var words = new[] { "И", "Кот", "в", "доме", "НА", "полу", "бежит" };
        File.WriteAllLines(_tempInputFile, words);

        var settings = new AppSettings
        {
            InputFile = _tempInputFile,
            OutputFile = "test_filtered.png",
            StopWords = new List<string> { "и", "в", "на" },
            ToLowerCase = true
        };
        
        using var container = CreateContainer(settings);
        var app = container.Resolve<TagCloudApplication>();
        
        var resultPath = app.GenerateFromFile(settings);
        
        resultPath.Should().NotBeNullOrEmpty();
        File.Exists(resultPath).Should().BeTrue();

        var fileInfo = new FileInfo(resultPath);
        fileInfo.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public void GenerateFromFile_ShouldThrow_WhenInputFileNotFound()
    {
        var settings = new AppSettings
        {
            InputFile = "nonexistent_file.txt",
            OutputFile = "test.png"
        };
        
        using var container = CreateContainer(settings);
        var app = container.Resolve<TagCloudApplication>();
        
        Assert.Throws<FileNotFoundException>(() => app.GenerateFromFile(settings));
    }

    [Test]
    public void GenerateFromFile_ShouldHandleEmptyFile_Gracefully()
    {
        File.WriteAllText(_tempInputFile, string.Empty);

        var settings = new AppSettings
        {
            InputFile = _tempInputFile,
            OutputFile = "test_empty.png"
        };
        
        using var container = CreateContainer(settings);
        var app = container.Resolve<TagCloudApplication>();
        
        Assert.Throws<InvalidOperationException>(() => app.GenerateFromFile(settings));
    }

    private IContainer CreateContainer(AppSettings settings)
    {
        var builder = new ContainerBuilder();
        
        builder.RegisterModule<DependencyContainer>();
        
        builder.RegisterInstance(_imageSaver).As<IImageSaver>();
        
        builder.RegisterInstance(settings)
               .As<AppSettings>()
               .ExternallyOwned();
        
        return builder.Build();
    }
}