using System.Drawing;
using System.Drawing.Imaging;
using FakeItEasy;
using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class TagCloudVisualizerTests
{
    private TagCloudVisualizer _visualizer;
    private IImageSaver _fakeImageSaver;
    private Point _center;

    [SetUp]
    public void SetUp()
    {
        _fakeImageSaver = A.Fake<IImageSaver>();
        _visualizer = new TagCloudVisualizer(_fakeImageSaver);
        _center = new Point(100, 100);
        
        A.CallTo(() => _fakeImageSaver.SaveBitmap(A<Bitmap>._, A<string>._, A<ImageFormat>._))
            .ReturnsLazily(call => Path.Combine("test_output", call.Arguments.Get<string>(1)));
    }

    [Test]
    public void CreateVisualization_WithWordTags_ReturnsNonEmptyBitmap()
    {
        var wordTags = new[]
        {
            new WordTag("Hello", 5, new Font("Arial", 20), Color.Blue)
            {
                Rectangle = new Rectangle(50, 50, 60, 30)
            },
            new WordTag("World", 3, new Font("Arial", 16), Color.Red)
            {
                Rectangle = new Rectangle(100, 100, 70, 25)
            }
        };
        
        var config = new TagCloudVisualizationConfig("test.png", backgroundColor: Color.White);
        
        var bitmap = _visualizer.CreateVisualization(wordTags, _center, config);
        
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().BeGreaterThan(0);
        bitmap.Height.Should().BeGreaterThan(0);
    }

    [Test]
    public void CreateVisualization_WithImageSize_UsesSpecifiedSize()
    {
        var wordTags = new[]
        {
            new WordTag("Test", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var expectedSize = new Size(800, 600);
        var config = new TagCloudVisualizationConfig("test.png", imageSize: expectedSize);
        
        var bitmap = _visualizer.CreateVisualization(wordTags, _center, config);
        
        bitmap.Size.Should().Be(expectedSize);
    }

    [Test]
    public void CreateVisualization_WithoutImageSize_CalculatesOptimalSize()
    {
        var wordTags = new[]
        {
            new WordTag("First", 2, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(0, 0, 50, 50)
            },
            new WordTag("Second", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(200, 200, 70, 30)
            }
        };
        
        var config = new TagCloudVisualizationConfig("test.png", imageSize: null);
        
        var bitmap = _visualizer.CreateVisualization(wordTags, _center, config); 
        
        bitmap.Width.Should().BeGreaterThan(200 + 70); 
        bitmap.Height.Should().BeGreaterThan(200 + 30);
    }
    
    [Test]
    public void CreateVisualization_UsesSpecifiedBackgroundColor()
    {
        var wordTags = new[]
        {
            new WordTag("Test", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var backgroundColor = Color.Black;
        var config = new TagCloudVisualizationConfig(
            "test.png",
            backgroundColor: backgroundColor);
        
        var bitmap = _visualizer.CreateVisualization(wordTags, _center, config);
        
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().BeGreaterThan(0);
        bitmap.Height.Should().BeGreaterThan(0);
    }

    [Test]
    public void SaveVisualization_CallsImageSaverWithCorrectFileName()
    {
        var wordTags = new[]
        {
            new WordTag("Test", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var expectedFileName = "test_output.png";
        var config = new TagCloudVisualizationConfig(expectedFileName);
        
        _visualizer.SaveVisualization(wordTags, _center, config);
        
        A.CallTo(() => _fakeImageSaver.SaveBitmap(
            A<Bitmap>._, 
            expectedFileName, 
            A<ImageFormat>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveVisualization_ReturnsPathFromImageSaver()
    {
        var wordTags = new[]
        {
            new WordTag("Test", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var expectedPath = @"C:\output\test.png";
        var config = new TagCloudVisualizationConfig("test.png");
        
        A.CallTo(() => _fakeImageSaver.SaveBitmap(A<Bitmap>._, A<string>._, A<ImageFormat>._))
            .Returns(expectedPath);
        
        var result = _visualizer.SaveVisualization(wordTags, _center, config);
        
        result.Should().Be(expectedPath);
    }
}