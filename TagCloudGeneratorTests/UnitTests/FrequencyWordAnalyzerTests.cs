using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class FrequencyWordAnalyzerTests
{
    private FrequencyWordAnalyzer _analyzer;

    [SetUp]
    public void SetUp()
    {
        _analyzer = new FrequencyWordAnalyzer();
    }

    [Test]
    public void Analyze_ShouldCountWordFrequencies_CaseInsensitive()
    {
        var words = new[] { "кот", "собака", "Кот", "СОБАКА", "кот", "Собака" };
        
        var result = _analyzer.Analyze(words);
        result.IsSuccess.Should().BeTrue();
        var frequencies = result.GetValueOrThrow();
        
        frequencies.Should().HaveCount(2);
        frequencies["кот"].Should().Be(3);
        frequencies["собака"].Should().Be(3);
    }

    [Test]
    public void Analyze_ShouldReturnEmptyDictionary_ForEmptyInput()
    {
        var words = Enumerable.Empty<string>();
        
        var result = _analyzer.Analyze(words);
        result.IsSuccess.Should().BeTrue();
        var frequencies = result.GetValueOrThrow();
        
        frequencies.Should().BeEmpty();
        frequencies.Should().NotBeNull();
    }

    [Test]
    public void Analyze_ShouldHandleDuplicateWords()
    {
        var words = new[] { "тест", "тест", "тест", "тест" };
        
        var result = _analyzer.Analyze(words);
        result.IsSuccess.Should().BeTrue();
        var frequencies = result.GetValueOrThrow();
        
        frequencies.Should().HaveCount(1);
        frequencies["тест"].Should().Be(4);
    }
}