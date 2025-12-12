namespace TagCloudGenerator;

using System.Drawing;
using System.Text.RegularExpressions;

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

    public static bool TryParseColor(string colorString, out Color? outColor)
    {
        outColor = null;
        
        if (string.IsNullOrWhiteSpace(colorString))
            return false;
        
        if (TryParseNamedColor(colorString, out var namedColor))
        {
            outColor = namedColor;
            return true;
        }
        
        if (TryParseHexColor(colorString, out var hexColor))
        {
            outColor = hexColor;
            return true;
        }
        
        if (TryParseRgbColor(colorString, out var rgbColor))
        {
            outColor = rgbColor;
            return true;
        }
        
        if (!TryParseArgbColor(colorString, out var argbColor)) 
            return false;
        outColor = argbColor;
        return true;

    }

    private static bool TryParseNamedColor(string colorString, out Color? color)
    {
        color = null;
        
        var colorProperty = typeof(Color)
            .GetProperty(colorString, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Static | 
                System.Reflection.BindingFlags.IgnoreCase);

        if (colorProperty == null || colorProperty.PropertyType != typeof(Color)) 
            return false;
        color = (Color)colorProperty.GetValue(null)!;
        return true;

    }

    private static bool TryParseHexColor(string colorString, out Color? color)
    {
        color = null;
        
        if (!HexColorRegex.IsMatch(colorString))
            return false;
        
        var hex = colorString.StartsWith("#") 
            ? colorString.Substring(1) 
            : colorString;
        
        try
        {
            switch (hex.Length)
            {
                case 3:
                    var r3 = Convert.ToInt32(hex.Substring(0, 1) + hex.Substring(0, 1), 16);
                    var g3 = Convert.ToInt32(hex.Substring(1, 1) + hex.Substring(1, 1), 16);
                    var b3 = Convert.ToInt32(hex.Substring(2, 1) + hex.Substring(2, 1), 16);
                    color = Color.FromArgb(r3, g3, b3);
                    return true;
                    
                case 6:
                    var r6 = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g6 = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b6 = Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = Color.FromArgb(r6, g6, b6);
                    return true;
                    
                case 8: 
                    var a8 = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var r8 = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var g8 = Convert.ToInt32(hex.Substring(4, 2), 16);
                    var b8 = Convert.ToInt32(hex.Substring(6, 2), 16);
                    color = Color.FromArgb(a8, r8, g8, b8);
                    return true;
            }
        }
        catch
        {
            return false;
        }
        
        return false;
    }

    private static bool TryParseRgbColor(string colorString, out Color? color)
    {
        color = null;
        
        var match = RgbRegex.Match(colorString);
        if (!match.Success)
            return false;
        
        if (!int.TryParse(match.Groups[1].Value, out var r) ||
            !int.TryParse(match.Groups[2].Value, out var g) ||
            !int.TryParse(match.Groups[3].Value, out var b))
            return false;
        
        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            return false;
        
        color = Color.FromArgb(r, g, b);
        return true;
    }

    private static bool TryParseArgbColor(string colorString, out Color? color)
    {
        color = null;
        
        var match = ArgbRegex.Match(colorString);
        if (!match.Success)
            return false;
        
        if (!int.TryParse(match.Groups[1].Value, out var a) ||
            !int.TryParse(match.Groups[2].Value, out var r) ||
            !int.TryParse(match.Groups[3].Value, out var g) ||
            !int.TryParse(match.Groups[4].Value, out var b))
            return false;
        
        if (a < 0 || a > 255 || r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            return false;
        
        color = Color.FromArgb(a, r, g, b);
        return true;
    }
}