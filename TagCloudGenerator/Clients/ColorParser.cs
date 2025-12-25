using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TagCloudGenerator;

public static class ColorParser
{
    private static readonly Regex HexColorRegex = new Regex(
        @"^#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex RgbRegex = new Regex(
        @"^\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*$", 
        RegexOptions.Compiled);
    
    private static readonly Regex ArgbRegex = new Regex(
        @"^\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*$", 
        RegexOptions.Compiled);

    public static Result<Color?> ParseColor(string colorString)
    {
        if (string.IsNullOrWhiteSpace(colorString))
            return Result.Ok<Color?>(null);
    
        return ParseNamedColor(colorString)
            .Then(color => color.HasValue 
                ? color.AsResult() 
                : ParseHexColor(colorString))
            .Then(color => color.HasValue 
                ? color.AsResult() 
                : ParseRgbColor(colorString))
            .Then(color => color.HasValue 
                ? color.AsResult() 
                : ParseArgbColor(colorString))
            .Then(color => 
            {
                if (color.HasValue)
                    return Result.Ok(color);
                return Result.Fail<Color?>($"Invalid color format: '{colorString}'. " +
                                               "Expected: color name (Red, Blue), hex (#FF0000), RGB (255,0,0), or ARGB (255,255,0,0)");
            });
    }

    private static Result<Color?> ParseNamedColor(string colorString)
    {
        var colorProperty = typeof(Color)
            .GetProperty(colorString, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Static | 
                System.Reflection.BindingFlags.IgnoreCase);

        if (colorProperty == null || colorProperty.PropertyType != typeof(Color))
            return Result.Ok<Color?>(null);
        
        var colorValue = (Color?)colorProperty.GetValue(null);
        return Result.Ok(colorValue);

    }

    private static Result<Color?> ParseHexColor(string colorString)
    {
        if (!HexColorRegex.IsMatch(colorString))
            return Result.Ok<Color?>(null);
        
        var hex = colorString.StartsWith("#") 
            ? colorString[1..] 
            : colorString;
        
        return hex.Length switch
        {
            3 => ParseShortHexColor(hex),
            6 => ParseFullHexColor(hex, false),
            8 => ParseFullHexColor(hex, true),
            _ => Result.Ok<Color?>(null)
        };
    }

    private static Result<Color?> ParseShortHexColor(string hex)
    {
        var r = ParseHexByte(hex.Substring(0, 1) + hex.Substring(0, 1));
        var g = ParseHexByte(hex.Substring(1, 1) + hex.Substring(1, 1));
        var b = ParseHexByte(hex.Substring(2, 1) + hex.Substring(2, 1));
        
        return r.Then(rValue => g.Then(gValue => b.Then(bValue => 
            Result.Ok<Color?>(Color.FromArgb(rValue, gValue, bValue)))));
    }

    private static Result<Color?> ParseFullHexColor(string hex, bool hasAlpha)
    {
        if (hasAlpha)
        {
            var a = ParseHexByte(hex.Substring(0, 2));
            var r = ParseHexByte(hex.Substring(2, 2));
            var g = ParseHexByte(hex.Substring(4, 2));
            var b = ParseHexByte(hex.Substring(6, 2));
            
            return a.Then(aValue => r.Then(rValue => g.Then(gValue => b.Then(bValue => 
                Result.Ok<Color?>(Color.FromArgb(aValue, rValue, gValue, bValue))))));
        }
        else
        {
            var r = ParseHexByte(hex.Substring(0, 2));
            var g = ParseHexByte(hex.Substring(2, 2));
            var b = ParseHexByte(hex.Substring(4, 2));
            
            return r.Then(rValue => g.Then(gValue => b.Then(bValue => 
                Result.Ok<Color?>(Color.FromArgb(rValue, gValue, bValue)))));
        }
    }

    private static Result<int> ParseHexByte(string hex)
    {
        return int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result)
            ? Result.Ok(result)
            : Result.Fail<int>($"Invalid hex byte: {hex}");
    }

    private static Result<Color?> ParseRgbColor(string colorString)
    {
        var match = RgbRegex.Match(colorString);
        if (!match.Success)
            return Result.Ok<Color?>(null);
        
        var r = ParseNumber(match.Groups[1].Value, 0, 255);
        var g = ParseNumber(match.Groups[2].Value, 0, 255);
        var b = ParseNumber(match.Groups[3].Value, 0, 255);
        
        return r.Then(rValue => g.Then(gValue => b.Then(bValue => 
            Result.Ok<Color?>(Color.FromArgb(rValue, gValue, bValue)))));
    }

    private static Result<Color?> ParseArgbColor(string colorString)
    {
        var match = ArgbRegex.Match(colorString);
        if (!match.Success)
            return Result.Ok<Color?>(null);
        
        var a = ParseNumber(match.Groups[1].Value, 0, 255);
        var r = ParseNumber(match.Groups[2].Value, 0, 255);
        var g = ParseNumber(match.Groups[3].Value, 0, 255);
        var b = ParseNumber(match.Groups[4].Value, 0, 255);
        
        return a.Then(aValue => r.Then(rValue => g.Then(gValue => b.Then(bValue => 
            Result.Ok<Color?>(Color.FromArgb(aValue, rValue, gValue, bValue))))));
    }

    private static Result<int> ParseNumber(string text, int min, int max)
    {
        return int.TryParse(text, out var number) && number >= min && number <= max
            ? Result.Ok(number)
            : Result.Fail<int>($"Value must be between {min} and {max}");
    }
}