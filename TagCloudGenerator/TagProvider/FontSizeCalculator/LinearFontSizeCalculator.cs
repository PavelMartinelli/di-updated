namespace TagCloudGenerator;

public class LinearFontSizeCalculator : IFontSizeCalculator
{
    public float CalculateFontSize(int frequency, int minFontSize, int maxFontSize)
    {
        return Math.Max(minFontSize, Math.Min(maxFontSize, minFontSize + frequency));
    }
}