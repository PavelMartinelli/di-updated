using System.Drawing;
using FakeItEasy;
using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class TagProviderTests
{
    private ITagProvider _tagProvider;
    private IFontSizeCalculator _mockFontSizeCalculator;
    private IColorScheme _mockColorScheme;

    [SetUp]
    public void SetUp()
    {
        _mockFontSizeCalculator = A.Fake<IFontSizeCalculator>();
        _mockColorScheme = A.Fake<IColorScheme>();
        
        _tagProvider = new TagProvider();
            
        A.CallTo(() => _mockFontSizeCalculator.CalculateFontSize(A<int>._, A<int>._, A<int>._))
            .ReturnsLazily((int freq, int min, int max) => min + freq * 2);
            
        A.CallTo(() => _mockColorScheme.GetColorForWord(A<WordTag>._, A<int>._, A<int>._))
            .Returns(Color.Blue);
    }

    [Test]
    public void GetTags_ShouldOrderByFrequencyDescending()
    {
        var wordFrequencies = new Dictionary<string, int>
        {
            ["яблоко"] = 5,
            ["банан"] = 10,
            ["апельсин"] = 3
        };
        
        var config = new TagCloudProviderConfig(
            new Point(400, 300),
            wordFrequencies,
            "Arial",
            20,
            60);

        var tags = _tagProvider.GetTags(config, _mockFontSizeCalculator, _mockColorScheme)
            .ToList();

        tags.Should().HaveCount(3);
        tags[0].Text.Should().Be("банан");
        tags[1].Text.Should().Be("яблоко");
        tags[2].Text.Should().Be("апельсин");
    }

    [Test]
    public void GetTags_ShouldApplyFontSizeCalculator()
    {
        var wordFrequencies = new Dictionary<string, int>
        {
            ["тест"] = 5
        };
        
        var config = new TagCloudProviderConfig(
            new Point(400, 300),
            wordFrequencies,
            "Arial",
            20,
            60);

        var tag = _tagProvider.GetTags(config, _mockFontSizeCalculator, _mockColorScheme)
            .First();

        tag.Font.Size.Should().Be(30);
        tag.Font.FontFamily.Name.Should().Be("Arial");
    }
}