namespace TagCloudGenerator;

public interface IFontSizeCalculator
{
    float CalculateFontSize(int frequency, int minFontSize, int maxFontSize);
}