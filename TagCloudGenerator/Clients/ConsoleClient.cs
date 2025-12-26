using Autofac;
using CommandLine;
using System.Drawing;

namespace TagCloudGenerator;

public class ConsoleClient
{
    public static int Run(string[] args)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DependencyContainer>();
        
        builder.RegisterInstance(new AppSettings())
               .As<AppSettings>()
               .ExternallyOwned();
        
        var container = builder.Build();
        
        return Parser.Default.ParseArguments<GenerateOptions, HelpOptions>(args)
            .MapResult(
                (GenerateOptions opts) => RunGenerate(opts, container),
                (HelpOptions opts) => DisplayHelp(),
                errs => 
                {
                    Console.WriteLine("\n Invalid command line arguments");
                    DisplayHelp();
                    return 1;
                });
    }
    
    private static int RunGenerate(GenerateOptions opts, IContainer container)
    {
        Console.WriteLine("Tag Cloud Generator");
        Console.WriteLine("===================");
        
        var settingsResult = CreateSettings(opts)
            .OnFail(error => Console.WriteLine($"\n Error: {error}"));
        
        if (!settingsResult.IsSuccess)
            return 1;
        
        var settings = settingsResult.Value;
        
        Console.WriteLine($"Input file: {settings.InputFile}");
        Console.WriteLine($"Output file: {settings.OutputFile}");
        Console.WriteLine($"Image size: {settings.Width}x{settings.Height}");
        Console.WriteLine($"Font: {settings.FontFamily} ({settings.MinFontSize}-{settings.MaxFontSize}pt)");
        Console.WriteLine($"Color scheme: {settings.ColorScheme}");
        Console.WriteLine($"To lowercase: {settings.ToLowerCase}");
        Console.WriteLine($"Stop words: {settings.StopWords.Count} loaded");
        
        using var scope = container.BeginLifetimeScope(b =>
        {
            b.RegisterInstance(settings).As<AppSettings>().ExternallyOwned();
        });
        
        var tagCloudApp = scope.Resolve<TagCloudApplication>();
        
        Console.WriteLine("\nGenerating tag cloud...");
        var result = tagCloudApp.GenerateFromFile(settings)
            .Then(path =>
            {
                Console.WriteLine($"\n Success! Tag cloud saved to:");
                Console.WriteLine($"   {path}");
            })
            .OnFail(error =>
            {
                Console.WriteLine($"\n Error: {error}");
            });
        
        return result.IsSuccess ? 0 : 1;
    }
    
    private static Result<AppSettings> CreateSettings(GenerateOptions opts)
    {
        return LoadStopWords(opts.StopWordsFile)
            .Then(stopWords => ParseBackgroundColor(opts.BackgroundColor)
                .Then(backgroundColor => new AppSettings
                {
                    InputFile = opts.InputFile,
                    OutputFile = opts.OutputFile,
                    Width = opts.Width,
                    Height = opts.Height,
                    FontFamily = opts.FontFamily,
                    MinFontSize = opts.MinFontSize,
                    MaxFontSize = opts.MaxFontSize,
                    BackgroundColor = backgroundColor,
                    ColorScheme = opts.ColorScheme,
                    Algorithm = opts.Algorithm,
                    Center = new Point(opts.CenterX, opts.CenterY),
                    StopWords = stopWords,
                    ToLowerCase = !opts.NoLowerCase
                }.AsResult()));
    }
    
    private static Result<List<string>> LoadStopWords(string stopWordsFile)
    {
        if (string.IsNullOrEmpty(stopWordsFile))
            return new List<string>().AsResult();
        
        return Result.Of(() =>
        {
            if (!File.Exists(stopWordsFile))
                throw new FileNotFoundException($"Stop words file not found: {stopWordsFile}");
            
            return File.ReadAllLines(stopWordsFile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();
        }).ReplaceError(err => $"Cannot load stop words from '{stopWordsFile}': {err}"); 
    }
    
    private static Result<Color> ParseBackgroundColor(string colorString)
    {
        return ColorParser.ParseColor(colorString)
            .Then(color => color ?? AppSettings.DefaultBackgroundColor);
    }
    
    private static int DisplayHelp()
    {
        Console.WriteLine("Tag Cloud Generator");
        Console.WriteLine("===================\n");
        Console.WriteLine("Generates a tag cloud visualization from a text file.\n");
        Console.WriteLine("Usage: TagCloudVisualization.exe [command] [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  generate    Generate tag cloud");
        Console.WriteLine("  help        Display this help message\n");
        Console.WriteLine("Examples:");
        Console.WriteLine("  TagCloudVisualization.exe generate -i words.txt");
        Console.WriteLine("  TagCloudVisualization.exe generate -i words.txt -o cloud.png -w 1200 -h 900");
        Console.WriteLine("  TagCloudVisualization.exe generate -i words.txt --color-scheme Frequency --max-font 80");
        Console.WriteLine("  TagCloudVisualization.exe generate -i words.txt --stop-words stopwords.txt");
        
        return 0;
    }
}