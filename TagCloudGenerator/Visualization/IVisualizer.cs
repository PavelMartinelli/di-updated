namespace TagCloudGenerator;

using System.Drawing;

public interface IVisualizer
{
    Result<string> SaveVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config);
}