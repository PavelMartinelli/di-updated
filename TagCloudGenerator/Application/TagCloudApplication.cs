namespace TagCloudGenerator;

using System.Drawing;

public class TagCloudApplication
{
    private readonly ITextReader _textReader;
    private readonly IWordPreprocessor _wordPreprocessor;
    private readonly IWordAnalyzer _wordAnalyzer;
    private readonly ITagProvider _tagProvider;
    private readonly ITagCloudArrangeAlgorithm _algorithm;
    private readonly IColorScheme _colorScheme;
    private readonly ITextMeasurer _textMeasurer;
    private readonly IFontSizeCalculator _fontSizeCalculator;
    private readonly Func<Point, ICloudLayouter> _layouterFactory;
    private readonly IVisualizer _visualizer;

    public TagCloudApplication(
        ITextReader textReader,
        IWordPreprocessor wordPreprocessor,
        IWordAnalyzer wordAnalyzer,
        ITagProvider tagProvider,
        ITagCloudArrangeAlgorithm algorithm,
        IColorScheme colorScheme,
        ITextMeasurer textMeasurer,
        IFontSizeCalculator fontSizeCalculator,
        Func<Point, ICloudLayouter> layouterFactory,
        IVisualizer visualizer)
    {
        _textReader = textReader ?? throw new ArgumentNullException(nameof(textReader));
        _wordPreprocessor = wordPreprocessor ?? throw new ArgumentNullException(nameof(wordPreprocessor));
        _wordAnalyzer = wordAnalyzer ?? throw new ArgumentNullException(nameof(wordAnalyzer));
        _tagProvider = tagProvider ?? throw new ArgumentNullException(nameof(tagProvider));
        _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        _colorScheme = colorScheme ?? throw new ArgumentNullException(nameof(colorScheme));
        _textMeasurer = textMeasurer ?? throw new ArgumentNullException(nameof(textMeasurer));
        _fontSizeCalculator = fontSizeCalculator ?? throw new ArgumentNullException(nameof(fontSizeCalculator));
        _layouterFactory = layouterFactory ?? throw new ArgumentNullException(nameof(layouterFactory));
        _visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
    }

    public Result<string> GenerateFromFile(AppSettings settings)
    {
        return settings.Validate()
            .Then(_ => ValidateFont(settings.FontFamily))
            .Then(_ => _textReader.ReadLines(settings.InputFile))
            .Then(lines => _wordPreprocessor.Process(lines))
            .Then(processedWords => _wordAnalyzer.Analyze(processedWords))
            .Then(ValidateWordFrequencies)
            .Then(wordFrequencies => CreateGenerationConfig(settings, wordFrequencies).AsResult())
            .Then(config => _tagProvider.GetTags(config, _fontSizeCalculator, _colorScheme))
            .Then(tags => tags.ToList().AsResult())
            .Then(tags => ArrangeTags(settings.Center, tags))
            .Then(arrangedTags => ValidateTagsFitImage(arrangedTags, settings.Width, settings.Height))
            .Then(arrangedTags => SaveVisualization(settings, arrangedTags));
    }

    private Result<None> ValidateFont(string fontFamily)
    {
        return Result.OfAction(() =>
            {
                using var testFont = new Font(fontFamily, 12);
                if (!testFont.Name.Equals(fontFamily, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Font '{fontFamily}' is not available in the system.");
            }, $"Font '{fontFamily}' is not available in the system. Please check the font name and try again.");
    }

    private Result<Dictionary<string, int>> ValidateWordFrequencies(Dictionary<string, int> wordFrequencies)
    {
        return wordFrequencies.Any()
            ? Result.Ok(wordFrequencies)
            : Result.Fail<Dictionary<string, int>>("No words found in the input file after preprocessing");
    }

    private TagCloudProviderConfig CreateGenerationConfig(AppSettings settings, Dictionary<string, int> wordFrequencies)
    {
        return new TagCloudProviderConfig(
            settings.Center,
            wordFrequencies,
            settings.FontFamily,
            settings.MinFontSize,
            settings.MaxFontSize);
    }

    private Result<List<WordTag>> ArrangeTags(Point center, List<WordTag> tags)
    {
        var layouter = _layouterFactory(center);
        return _algorithm.ArrangeTags(tags, layouter, _textMeasurer)
            .Then(arrangedTags => arrangedTags.ToList().AsResult());
    }

    private Result<List<WordTag>> ValidateTagsFitImage(List<WordTag> tags, int width, int height)
    {
        if (!tags.Any())
            return Result.Ok(tags);

        var minX = tags.Min(t => t.Rectangle.Left);
        var maxX = tags.Max(t => t.Rectangle.Right);
        var minY = tags.Min(t => t.Rectangle.Top);
        var maxY = tags.Max(t => t.Rectangle.Bottom);

        var actualWidth = maxX - minX;
        var actualHeight = maxY - minY;

        return (actualWidth <= width && actualHeight <= height)
            ? Result.Ok(tags)
            : Result.Fail<List<WordTag>>(
                $"Tag cloud ({actualWidth}x{actualHeight}) does not fit into specified image size ({width}x{height}). " +
                "Try increasing image dimensions, decreasing font sizes, or reducing the number of words.");
    }

    private Result<string> SaveVisualization(AppSettings settings, List<WordTag> arrangedTags)
    {
        var visualizationConfig = new TagCloudVisualizationConfig(
            settings.OutputFile,
            new Size(settings.Width, settings.Height),
            settings.BackgroundColor);
    
        return _visualizer.SaveVisualization(arrangedTags, settings.Center, visualizationConfig);
    }
}