namespace TagCloudGenerator;

public class SpiralTagCloudAlgorithm : ITagCloudArrangeAlgorithm
{
    public Result<IEnumerable<WordTag>> ArrangeTags(
        IEnumerable<WordTag> tags,
        ICloudLayouter layouter,
        ITextMeasurer textMeasurer)
    {
        var result = new List<WordTag>();
        
        foreach (var tag in tags.OrderByDescending(t => t.Frequency))
        {
            var size = textMeasurer.MeasureString(tag.Text, tag.Font);
            var rectangleResult = layouter.PutNextRectangle(size);
            
            if (!rectangleResult.IsSuccess)
                return Result.Fail<IEnumerable<WordTag>>(
                    $"Failed to arrange tag '{tag.Text}': {rectangleResult.Error}");
            
            tag.Rectangle = rectangleResult.Value;
            result.Add(tag);
        }
        
        return result.AsEnumerable().AsResult();
    }
}