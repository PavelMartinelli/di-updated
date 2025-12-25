namespace TagCloudGenerator;

public interface ITagProvider
{
    Result<IEnumerable<WordTag>> GetTags(
        TagCloudProviderConfig config,
        IFontSizeCalculator fontSizeCalculator,
        IColorScheme colorProvider);
}