using System.Drawing;
using FluentAssertions;
using NUnit.Framework.Interfaces;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class CircularCloudLayouterTests
{
    private ICloudLayouter _layouter;
    private Point _center;
    private TagCloudVisualizer _testVisualizer;
    private IImageSaver _imageSaver;
    private List<Rectangle> _placedRectangles;

    [SetUp]
    public void SetUp()
    {
        _center = new Point(100, 100);
        var spiralProvider = new SpiralPointsProvider(_center);
        _layouter = new CircularCloudLayouter(_center, spiralProvider);
        _imageSaver = new ImageSaver("test_results");
        _testVisualizer = new TagCloudVisualizer(_imageSaver);
        _placedRectangles = new List<Rectangle>();
    }
    
    [TearDown]
    public void TearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed) 
            return;
        
        try
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{testName}_{timestamp}.png";
            
            var wordTags = _placedRectangles.Select((rect, index) =>
            {
                var wordTag = new WordTag(
                    $"Word{index}", 
                    1, 
                    new Font("Arial", 12), 
                    Color.Black)
                {
                    Rectangle = rect
                };
                return wordTag;
            }).ToList();
            
            var config = new TagCloudVisualizationConfig(fileName);
            
            var saveResult = _testVisualizer.SaveVisualization(
                wordTags, 
                _center, 
                config
            );
            
            if (!saveResult.IsSuccess) 
                return;
            var filePath = saveResult.GetValueOrThrow();
            TestContext.Out.WriteLine($"Tag cloud visualization saved to file {filePath}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Failed to save visualization: {ex.Message}");
        }
    }

    [Test]
    public void PutNextRectangle_FirstRectangle_PlacedInCenter()
    {
        var size = new Size(10, 10);
        
        var result = _layouter.PutNextRectangle(size);
        result.IsSuccess.Should().BeTrue();
        var actual = result.GetValueOrThrow();
        _placedRectangles.Add(actual);
        
        var expected = new Rectangle(
            new Point(_center.X - size.Width / 2, _center.Y - size.Height / 2), 
            size);
        actual.Should().Be(expected);
    }

    [Test]
    public void PutNextRectangle_MultipleRectangles_DoNotIntersect()
    {
        for (var i = 0; i < 5; i++)
        {
            var result = _layouter.PutNextRectangle(new Size(45, 15));
            result.IsSuccess.Should().BeTrue();
            var rectangle = result.GetValueOrThrow();
            _placedRectangles.Add(rectangle);
        }
        
        _placedRectangles.Should().NotBeEmpty();
        
        for (var i = 0; i < _placedRectangles.Count; i++)
            for (var j = i + 1; j < _placedRectangles.Count; j++)
                _placedRectangles[i].IntersectsWith(_placedRectangles[j]).Should().BeFalse();
    }

    [Test]
    public void PutNextRectangle_ManyRectangles_LayoutIsDense()
    {
        var sizes = new[]
        {
            new Size(60, 20),
            new Size(45, 15),
            new Size(30, 10),
            new Size(20, 40),
            new Size(15, 5)
        };
        
        var random = new Random(42);
        for (var i = 0; i < 200; i++)
        {
            var size = sizes[random.Next(sizes.Length)];
            var result = _layouter.PutNextRectangle(size);
            result.IsSuccess.Should().BeTrue();
            var rectangle = result.GetValueOrThrow();
            _placedRectangles.Add(rectangle);
        }
    
        const double minDensityRatio = 0.5;
    
        var totalRectanglesArea = _placedRectangles
            .Sum(rectangle => rectangle.Width * rectangle.Height);
        
        var boundingCircleRadius = CalculateBoundingCircleRadius(_placedRectangles);
        var boundingCircleArea = Math.PI * boundingCircleRadius * boundingCircleRadius;
        
        var density = totalRectanglesArea / boundingCircleArea;
        TestContext.Out.WriteLine($"Actual density: {density:F3}");
        density.Should().BeGreaterThan(minDensityRatio);
    }
    
    [Test]
    public void PutNextRectangle_ManyRectangles_RectanglesInAllQuadrants()
    {
        for (var i = 0; i < 150; i++)
        {
            var result = _layouter.PutNextRectangle(new Size(45, 15));
            result.IsSuccess.Should().BeTrue();
            var rectangle = result.GetValueOrThrow();
            _placedRectangles.Add(rectangle);
        }

        var quadrants = new int[4];

        foreach (var rect in _placedRectangles)
        {
            var rectCenter = new Point(rect.X + rect.Width / 2,
                rect.Y + rect.Height / 2);

            if (rectCenter.X >= _center.X && rectCenter.Y <= _center.Y)
                quadrants[0]++;
            else if (rectCenter.X >= _center.X && rectCenter.Y > _center.Y)
                quadrants[1]++;
            else if (rectCenter.X < _center.X && rectCenter.Y > _center.Y)
                quadrants[2]++;
            else
                quadrants[3]++;
        }
        
        quadrants.All(count => count > 0).Should().BeTrue(
            "Rectangles should be distributed in all quadrants to form a circular shape");
    }

    [Test]
    public void PutNextRectangle_ManyRectangles_DistributionIsRelativelyUniform()
    {
        for (var i = 0; i < 150; i++)
        {
            var result = _layouter.PutNextRectangle(new Size(45, 15));
            result.IsSuccess.Should().BeTrue();
            var rectangle = result.GetValueOrThrow();
            _placedRectangles.Add(rectangle);
        }
        
        var quadrants = new int[4];
    
        foreach (var rect in _placedRectangles)
        {
            var rectCenter = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            
            if (rectCenter.X >= _center.X && rectCenter.Y <= _center.Y) 
                quadrants[0]++;
            else if (rectCenter.X >= _center.X && rectCenter.Y > _center.Y) 
                quadrants[1]++;
            else if (rectCenter.X < _center.X && rectCenter.Y > _center.Y) 
                quadrants[2]++;
            else 
                quadrants[3]++;
        }
        
        var maxCount = quadrants.Max();
        var minCount = quadrants.Min();
        
        (maxCount <= minCount * 2).Should().BeTrue("Distribution across quadrants should be relatively uniform");
        
        TestContext.Out.WriteLine($"Distribution across quadrants: [{string.Join(", ", quadrants)}]");
        TestContext.Out.WriteLine($"Max: {maxCount}, Min: {minCount}, Ratio: {(double)maxCount / minCount:F2}");
    }

    private double CalculateBoundingCircleRadius(List<Rectangle> rectangles)
    {
        return rectangles
            .SelectMany(rect => new[]
            {
                GetDistance(rect.Location, _center),
                GetDistance(new Point(rect.Right, rect.Top), _center),
                GetDistance(new Point(rect.Left, rect.Bottom), _center),
                GetDistance(new Point(rect.Right, rect.Bottom), _center)
            })
            .Max();
    }

    private double GetDistance(Point point1, Point point2)
    {
        return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
    }
}