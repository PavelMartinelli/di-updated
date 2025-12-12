using System.Drawing;

namespace TagCloudGenerator;

public class SpiralPointsProvider : ISpiralPointsProvider
{
    private readonly Point _center;
    private readonly double _radiusStepFactor;
    
    private double _currentRadius;
    private double _currentAngle;

    public SpiralPointsProvider(Point center, double angleStep = 0.05, double radiusStepFactor = 0.05)
    {
        _center = center;
        _radiusStepFactor = radiusStepFactor;
        _currentRadius = 0;
        _currentAngle = 0;
    }

    public IEnumerable<Point> GetSpiralPoints(int minDimension)
    {
        var goldenAngle = Math.PI * (3 - Math.Sqrt(5));
        
        while (true)
        {
            _currentAngle += goldenAngle;
            _currentRadius = minDimension * _radiusStepFactor * (_currentAngle / (2 * Math.PI));
            
            var x = _center.X + (int)(_currentRadius * Math.Cos(_currentAngle));
            var y = _center.Y + (int)(_currentRadius * Math.Sin(_currentAngle));
            
            yield return new Point(x, y);
        }
    }
}