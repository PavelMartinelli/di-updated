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
        
        result.Should().HaveCount(2);
        result["кот"].Should().Be(3);
        result["собака"].Should().Be(3);
    }

    [Test]
    public void Analyze_ShouldReturnEmptyDictionary_ForEmptyInput()
    {
        var words = Enumerable.Empty<string>();
        
        var result = _analyzer.Analyze(words);
        
        result.Should().BeEmpty();
        result.Should().NotBeNull();
    }

    [Test]
    public void Analyze_ShouldHandleDuplicateWords()
    {
        var words = new[] { "тест", "тест", "тест", "тест" };
        
        var result = _analyzer.Analyze(words);
        
        result.Should().HaveCount(1);
        result["тест"].Should().Be(4);
    }
}