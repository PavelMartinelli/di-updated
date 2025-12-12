using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class WordPreprocessorTests
{
    [Test]
    public void Process_ShouldFilterStopWords_CaseInsensitive()
    {
        var stopWords = new List<string> { "the", "AND", "Is" };
        var preprocessor = new WordPreprocessor(stopWords, toLower: true);
        var words = new[] { "THE", "Cat", "and", "the", "DOG", "IS", "running" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().Equal("cat", "dog", "running");
    }

    [Test]
    public void Process_ShouldHandleEmptyStopWordsList()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "hello", "world", "test" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().Equal("hello", "world", "test");
    }
    
    [Test]
    public void Process_ShouldUseDefaultStopWords_WhenEmptyListProvided()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "the", "quick", "brown", "fox", "and", "the", "lazy", "dog" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().NotContain("the");
        result.Should().NotContain("and");
        result.Should().HaveCount(5);
    }

}