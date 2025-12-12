namespace TagCloudGenerator;

public interface ITagProvider
{
    IEnumerable<WordTag> GetTags(
        TagCloudProviderConfig config,
        IFontSizeCalculator fontSizeCalculator,
        IColorScheme colorProvider);
}