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
            .ReturnsLazily(call => Result.Ok(Path.Combine("test_output", call.Arguments.Get<string>(1))));
    }

    [Test]
    public void SaveVisualization_WithWordTags_ReturnsSuccess()
    {
        var wordTags = new[]
        {
            new WordTag("Привет", 5, new Font("Arial", 20), Color.Blue)
            {
                Rectangle = new Rectangle(50, 50, 60, 30)
            },
            new WordTag("Мир", 3, new Font("Arial", 16), Color.Red)
            {
                Rectangle = new Rectangle(100, 100, 70, 25)
            }
        };
        
        var config = new TagCloudVisualizationConfig("test.png", backgroundColor: Color.White);
        
        var result = _visualizer.SaveVisualization(wordTags, _center, config);
        
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void SaveVisualization_CallsImageSaverWithCorrectFileName()
    {
        var wordTags = new[]
        {
            new WordTag("Тест", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var expectedFileName = "test_output.png";
        var config = new TagCloudVisualizationConfig(expectedFileName);
        
        var result = _visualizer.SaveVisualization(wordTags, _center, config);
        result.IsSuccess.Should().BeTrue();
        
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
            new WordTag("Тест", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var expectedPath = @"C:\output\test.png";
        var config = new TagCloudVisualizationConfig("test.png");
        
        A.CallTo(() => _fakeImageSaver.SaveBitmap(A<Bitmap>._, A<string>._, A<ImageFormat>._))
            .Returns(Result.Ok(expectedPath));
        
        var result = _visualizer.SaveVisualization(wordTags, _center, config);
        
        result.IsSuccess.Should().BeTrue();
        result.GetValueOrThrow().Should().Be(expectedPath);
    }
    
    [Test]
    public void SaveVisualization_ReturnsFailure_WhenImageSaverFails()
    {
        var wordTags = new[]
        {
            new WordTag("Тест", 1, new Font("Arial", 12), Color.Black)
            {
                Rectangle = new Rectangle(50, 50, 40, 20)
            }
        };
        
        var config = new TagCloudVisualizationConfig("test.png");
        
        A.CallTo(() => _fakeImageSaver.SaveBitmap(A<Bitmap>._, A<string>._, A<ImageFormat>._))
            .Returns(Result.Fail<string>("Failed to save image"));
        
        var result = _visualizer.SaveVisualization(wordTags, _center, config);
        
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to save image");
    }
}