using System.Drawing;

namespace TagCloudGenerator;

public interface IVisualizer
{
    Result<string> SaveVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config);
}