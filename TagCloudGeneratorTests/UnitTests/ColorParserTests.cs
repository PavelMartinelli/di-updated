using System.Drawing;
using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class ColorParserTests
{
    [Test]
    public void ParseColor_ShouldReturnSuccessWithNull_ForNullOrEmptyString()
    {
        var result1 = ColorParser.ParseColor(null);
        result1.IsSuccess.Should().BeTrue();
        result1.GetValueOrThrow().Should().BeNull();
        
        var result2 = ColorParser.ParseColor("");
        result2.IsSuccess.Should().BeTrue();
        result2.GetValueOrThrow().Should().BeNull();
    }
    
    [Test]
    public void ParseColor_ShouldReturnFailure_ForInvalidFormat()
    {
        var result = ColorParser.ParseColor("invalid");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid color format");
    }

    [Test]
    public void ParseColor_ShouldParseNamedColors()
    {
        var result1 = ColorParser.ParseColor("Red");
        result1.IsSuccess.Should().BeTrue();
        result1.GetValueOrThrow().Value.ToArgb().Should().Be(Color.Red.ToArgb());
        
        var result2 = ColorParser.ParseColor("GREEN");
        result2.IsSuccess.Should().BeTrue();
        result2.GetValueOrThrow().Value.ToArgb().Should().Be(Color.Green.ToArgb());
    }

    [Test]
    public void ParseColor_ShouldParseHexColors()
    {
        var result1 = ColorParser.ParseColor("#FF0000");
        result1.IsSuccess.Should().BeTrue();
        result1.GetValueOrThrow().Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
        
        var result2 = ColorParser.ParseColor("#00FF00");
        result2.IsSuccess.Should().BeTrue();
        result2.GetValueOrThrow().Value.ToArgb().Should().Be(Color.FromArgb(255, 0, 255, 0).ToArgb());
    }

    [Test]
    public void ParseColor_ShouldParseRgbColors()
    {
        var result1 = ColorParser.ParseColor("255,0,0");
        result1.IsSuccess.Should().BeTrue();
        result1.GetValueOrThrow().Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
        
        var result2 = ColorParser.ParseColor("0,255,0");
        result2.IsSuccess.Should().BeTrue();
        result2.GetValueOrThrow().Value.ToArgb().Should().Be(Color.FromArgb(255, 0, 255, 0).ToArgb());
    }

    [Test]
    public void ParseColor_ShouldParseArgbColors()
    {
        var result = ColorParser.ParseColor("255,255,0,0");
        result.IsSuccess.Should().BeTrue();
        result.GetValueOrThrow().Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
    }
}