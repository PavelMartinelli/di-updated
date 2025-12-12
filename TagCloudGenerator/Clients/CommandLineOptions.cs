namespace TagCloudGenerator;

using CommandLine;


[Verb("generate", HelpText = "Generate a tag cloud from a text file")]
public class GenerateOptions
{
    [Option('i', "input", Required = true, HelpText = "Input text file with one word per line")]
    public string InputFile { get; set; }

    [Option('o', "output", Default = "tagcloud.png", HelpText = "Output image file")]
    public string OutputFile { get; set; }

    [Option('w', "width", Default = 1200, HelpText = "Image width in pixels")]
    public int Width { get; set; }

    [Option('h', "height", Default = 900, HelpText = "Image height in pixels")]
    public int Height { get; set; }

    [Option("font", Default = "Arial", HelpText = "Font family name")]
    public string FontFamily { get; set; }

    [Option("min-font", Default = 20, HelpText = "Minimum font size")]
    public int MinFontSize { get; set; }

    [Option("max-font", Default = 70, HelpText = "Maximum font size")]
    public int MaxFontSize { get; set; }

    [Option("bg-color", Default = "", HelpText = "Background color (name or hex)")]
    public string BackgroundColor { get; set; }

    [Option("color-scheme", Default = "Random", HelpText = "Color scheme: Random, Frequency, Gradient")]
    public string ColorScheme { get; set; }

    [Option("algorithm", Default = "Spiral", HelpText = "Algorithm: Spiral")]
    public string Algorithm { get; set; }

    [Option("center-x", Default = 600, HelpText = "Center X coordinate")]
    public int CenterX { get; set; }

    [Option("center-y", Default = 450, HelpText = "Center Y coordinate")]
    public int CenterY { get; set; } 

    [Option("stop-words", HelpText = "File with stop words (one per line)")]
    public string StopWordsFile { get; set; }

    [Option("no-lowercase", Default = false, HelpText = "Do not convert words to lowercase")]
    public bool NoLowerCase { get; set; }
}

[Verb("help", isDefault: true, HelpText = "Display help information")]
public class HelpOptions
{
}