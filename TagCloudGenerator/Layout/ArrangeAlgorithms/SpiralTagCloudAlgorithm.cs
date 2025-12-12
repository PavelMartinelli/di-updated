namespace TagCloudGenerator;

using System.Drawing;

public class SpiralTagCloudAlgorithm : ITagCloudArrangeAlgorithm
{
    public IEnumerable<WordTag> ArrangeTags(
        IEnumerable<WordTag> tags,
        ICloudLayouter layouter,
        ITextMeasurer textMeasurer)
    {
        var result = new List<WordTag>();
        
        foreach (var tag in tags.OrderByDescending(t => t.Frequency))
        {
            var size = textMeasurer.MeasureString(tag.Text, tag.Font);
            var rectangle = layouter.PutNextRectangle(size);
            tag.Rectangle = rectangle;
            result.Add(tag);
        }
        
        return result;
    }
}