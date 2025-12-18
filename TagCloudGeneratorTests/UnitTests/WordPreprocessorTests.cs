using FluentAssertions;
using TagCloudGenerator;

namespace TagCloudGeneratorTests;

[TestFixture]
public class WordPreprocessorTests
{
    [Test]
    public void Process_ShouldFilterStopWords_CaseInsensitive()
    {
        var stopWords = new List<string> { "и", "в", "на", "бежит" };
        var preprocessor = new WordPreprocessor(stopWords, toLower: true);
        var words = new[] { "И", "Кот", "в", "доме", "НА", "полу", "бежит" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().Equal("кот", "дом", "пол");
    }

    [Test]
    public void Process_ShouldHandleEmptyStopWordsList()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "привет", "мир", "тест" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().Equal("привет", "мир", "тест");
    }
    
    [Test]
    public void Process_ShouldUseDefaultStopWords_WhenEmptyListProvided()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "в", "лесу", "родилась", "и", "выросла", "ёлочка" };
        
        var result = preprocessor.Process(words).ToList();

        result.Should().NotContain("в");
        result.Should().NotContain("и");
        result.Should().HaveCount(4);
    }
    
     [Test]
    public void Process_ShouldLemmatizeWords_ToNormalForm()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "кошки", "кошкой", "кошкам", "кошке", "кошка" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.All(word => word == "кошка").Should().BeTrue();
        result.Should().HaveCount(5);
    }

    [Test]
    public void Process_ShouldFilterFunctionalPartsOfSpeech_WhenNoStopWordsProvided()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        var words = new[] { "и", "в", "под", "а", "чтобы", "ой" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().BeEmpty();
    }

    [Test]
    public void Process_ShouldReturnLowerCaseLemmas_WhenToLowerIsTrue()
    {
        var preprocessor = new WordPreprocessor(new List<string>(), toLower: true);
        
        var words = new[] { "СТОЛ", "Стол", "стол" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.All(word => word == "стол").Should().BeTrue();
        result.Should().HaveCount(3);
    }
    
    [Test]
    public void Process_ShouldUseCustomStopWords_WhenProvided_AndNotUseMorphology()
    {
        var customStopWords = new List<string> { "кошка", "собака" };
        var preprocessor = new WordPreprocessor(customStopWords, toLower: true);
        
        var words = new[] { "кошка", "и", "собака", "в", "доме" };
        
        var result = preprocessor.Process(words).ToList();
        
        result.Should().Contain("и");
        result.Should().Contain("в");
        result.Should().Contain("дом");
        result.Should().NotContain("кошка");
        result.Should().NotContain("собака");
        result.Should().HaveCount(3);
    }
}