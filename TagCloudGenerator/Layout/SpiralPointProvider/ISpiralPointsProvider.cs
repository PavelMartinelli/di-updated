using System.Drawing;

namespace TagCloudGenerator;

public interface ISpiralPointsProvider
{
    IEnumerable<Point> GetSpiralPoints(int minDimension);
}
