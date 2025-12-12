namespace TagCloudGenerator;

using System.Drawing;

public interface IVisualizer
{
    string SaveVisualization(IEnumerable<WordTag> tags, Point center, TagCloudVisualizationConfig config);
}