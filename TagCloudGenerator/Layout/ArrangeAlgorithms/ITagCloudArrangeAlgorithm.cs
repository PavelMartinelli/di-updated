namespace TagCloudGenerator;

public interface ITagCloudArrangeAlgorithm
{
    Result<IEnumerable<WordTag>> ArrangeTags(
        IEnumerable<WordTag> tags,
        ICloudLayouter layouter,
        ITextMeasurer textMeasurer);
}
