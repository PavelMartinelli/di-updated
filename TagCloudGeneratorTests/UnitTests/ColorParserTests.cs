using System.Drawing;
using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class ColorParserTests
{
    [Test]
    public void TryParseColor_ShouldReturnFalse_ForNullOrEmptyString()
    {
        ColorParser.TryParseColor(null, out var color).Should().BeFalse();
        color.Should().BeNull();
        
        ColorParser.TryParseColor("", out color).Should().BeFalse();
        color.Should().BeNull();
    }
    
    [Test]
    public void TryParseColor_ShouldReturnFalse_ForInvalidFormat()
    {
        ColorParser.TryParseColor("invalid", out var color).Should().BeFalse();
        color.Should().BeNull();
    }

    [Test]
    public void TryParseColor_ShouldParseNamedColors()
    {
        ColorParser.TryParseColor("Red", out var color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.Red.ToArgb());
        
        ColorParser.TryParseColor("GREEN", out color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.Green.ToArgb());
    }

    [Test]
    public void TryParseColor_ShouldParseHexColors()
    {
        ColorParser.TryParseColor("#FF0000", out var color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
        
        ColorParser.TryParseColor("#00FF00", out color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.FromArgb(255, 0, 255, 0).ToArgb());
    }

    [Test]
    public void TryParseColor_ShouldParseRgbColors()
    {
        ColorParser.TryParseColor("255,0,0", out var color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
        
        ColorParser.TryParseColor("0,255,0", out color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.FromArgb(255, 0, 255, 0).ToArgb());
    }

    [Test]
    public void TryParseColor_ShouldParseArgbColors()
    {
        ColorParser.TryParseColor("255,255,0,0", out var color).Should().BeTrue();
        color.Value.ToArgb().Should().Be(Color.FromArgb(255, 255, 0, 0).ToArgb());
    }
}