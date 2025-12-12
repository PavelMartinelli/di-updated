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

    public string GenerateFromFile(AppSettings settings)
    {
        if (!_textReader.CanRead(settings.InputFile))
            throw new FileNotFoundException($"Input file not found: {settings.InputFile}");
        
        var lines = _textReader.ReadLines(settings.InputFile);
    
        var processedWords = _wordPreprocessor.Process(lines);
    
        var wordFrequencies = _wordAnalyzer.Analyze(processedWords);
    
        if (!wordFrequencies.Any())
            throw new InvalidOperationException("No words found in the input file after preprocessing");
        
        var generationConfig = new TagCloudProviderConfig(
            settings.Center,
            wordFrequencies,
            settings.FontFamily,
            settings.MinFontSize,
            settings.MaxFontSize);
    
        var tags = _tagProvider.GetTags(
            generationConfig,
            _fontSizeCalculator,
            _colorScheme).ToList();
    
        var layouter = _layouterFactory(settings.Center);
    
        var arrangedTags = _algorithm.ArrangeTags(tags, layouter, _textMeasurer);
        
        var visualizationConfig = new TagCloudVisualizationConfig(
            settings.OutputFile,
            new Size(settings.Width, settings.Height),
            settings.BackgroundColor);
    
        return _visualizer.SaveVisualization(arrangedTags, settings.Center, visualizationConfig);
    }
}