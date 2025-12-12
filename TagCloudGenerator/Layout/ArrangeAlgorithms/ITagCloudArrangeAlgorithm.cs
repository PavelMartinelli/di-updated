namespace TagCloudGenerator;

public interface ITagCloudArrangeAlgorithm
{
    IEnumerable<WordTag> ArrangeTags(
        IEnumerable<WordTag> tags,
        ICloudLayouter layouter,
        ITextMeasurer textMeasurer);
}
