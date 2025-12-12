using System.Drawing;

namespace TagCloudGenerator;

public class CircularCloudLayouter : ICloudLayouter
{
    private readonly List<Rectangle> _placedRectangles;
    private readonly ISpiralPointsProvider _pointsProvider;
    private int _minDimension;
    private IEnumerator<Point> _spiralEnumerator;

    private Point Center { get; }

    public CircularCloudLayouter(Point center, ISpiralPointsProvider pointsProvider)
    {
        Center = center;
        _pointsProvider = pointsProvider;
        _placedRectangles = [];
        _minDimension = int.MaxValue;
        _spiralEnumerator = _pointsProvider.GetSpiralPoints(_minDimension).GetEnumerator();
    }

    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        if (rectangleSize.Width <= 0 || rectangleSize.Height <= 0)
            throw new ArgumentException("Rectangle size must have positive dimensions");

        UpdateMinDimension(rectangleSize);
        
        _spiralEnumerator ??= _pointsProvider.GetSpiralPoints(_minDimension)
            .GetEnumerator();
        
        while (true)
        {
            _spiralEnumerator.MoveNext();
            var point = _spiralEnumerator.Current;
            
            var candidateRectangle = CreateRectangleAtPoint(rectangleSize, point);

            if (IntersectsWithAny(candidateRectangle)) 
                continue;
            candidateRectangle = CompactRectangleTowardsCenter(candidateRectangle);
            _placedRectangles.Add(candidateRectangle);
            return candidateRectangle;
        }
    }

    private void UpdateMinDimension(Size size)
    {
        var newMinDimension = Math.Min(size.Width, size.Height);
        if (newMinDimension >= _minDimension) 
            return;
        
        _minDimension = newMinDimension;
        _spiralEnumerator = _pointsProvider.GetSpiralPoints(_minDimension).GetEnumerator();
    }

    private Rectangle CreateRectangleAtPoint(Size size, Point point)
    {
        return new Rectangle(
            point.X - size.Width / 2,
            point.Y - size.Height / 2,
            size.Width,
            size.Height);
    }

    private Rectangle CompactRectangleTowardsCenter(Rectangle rectangle)
    {
        var compacted = rectangle;
        var stepSize = Math.Max(1, Math.Min(rectangle.Width, rectangle.Height) / 10);
        
        while (CanMoveTowardsCenter(compacted, stepSize))
        {
            var direction = GetDirectionToCenter(compacted);
            var moved = MoveRectangle(compacted, direction, stepSize);
            
            if (!IntersectsWithAny(moved))
                compacted = moved;
            else
            {
                var movedX = new Rectangle(
                    compacted.X + Math.Sign(direction.X) * stepSize,
                    compacted.Y,
                    compacted.Width,
                    compacted.Height);
                    
                var movedY = new Rectangle(
                    compacted.X,
                    compacted.Y + Math.Sign(direction.Y) * stepSize,
                    compacted.Width,
                    compacted.Height);
                
                if (!IntersectsWithAny(movedX))
                    compacted = movedX;
                else if (!IntersectsWithAny(movedY))
                    compacted = movedY;
                else
                    break;
            }
            
            if (stepSize > 1)
                stepSize = Math.Max(1, stepSize / 2);
        }
        
        return compacted;
    }

    private bool CanMoveTowardsCenter(Rectangle rectangle, int stepSize)
    {
        var direction = GetDirectionToCenter(rectangle);
        if (direction.X == 0 && direction.Y == 0)
            return false;
            
        var moved = MoveRectangle(rectangle, direction, stepSize);
        return !IntersectsWithAny(moved);
    }

    private Point GetDirectionToCenter(Rectangle rectangle)
    {
        var rectCenter = new Point(
            rectangle.X + rectangle.Width / 2,
            rectangle.Y + rectangle.Height / 2);
            
        return new Point(
            Math.Sign(Center.X - rectCenter.X),
            Math.Sign(Center.Y - rectCenter.Y));
    }

    private Rectangle MoveRectangle(Rectangle rectangle, Point direction, int stepSize)
    {
        return new Rectangle(
            rectangle.X + direction.X * stepSize,
            rectangle.Y + direction.Y * stepSize,
            rectangle.Width,
            rectangle.Height);
    }

    private bool IntersectsWithAny(Rectangle rectangle)
    {
        foreach (var placed in _placedRectangles)
        {
            if (rectangle.IntersectsWith(placed))
                return true;
        }
        return false;
    }
}