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
                errs => 1);
    }
    
    private static int RunGenerate(GenerateOptions opts, IContainer container)
    {
        try
        {
            var stopWords = new List<string>();
            if (!string.IsNullOrEmpty(opts.StopWordsFile) && File.Exists(opts.StopWordsFile))
            {
                stopWords = File.ReadAllLines(opts.StopWordsFile)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();
            }

            var backgroundColor = ColorParser.TryParseColor(opts.BackgroundColor, out var parsedColor) 
                ? parsedColor.Value 
                : AppSettings.DefaultBackgroundColor;
            
            var settings = new AppSettings
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
            };
            
            Console.WriteLine("Tag Cloud Generator");
            Console.WriteLine("===================");
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
            var result = tagCloudApp.GenerateFromFile(settings);
            
            Console.WriteLine($"\n✅ Success! Tag cloud saved to:");
            Console.WriteLine($"   {result}");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
            return 1;
        }
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