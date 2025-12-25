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

    public Result<Rectangle> PutNextRectangle(Size rectangleSize)
    {
        return ValidateRectangleSize(rectangleSize)
            .Then(_ => UpdateMinDimension(rectangleSize))
            .Then(_ => FindPlaceForRectangle(rectangleSize))
            .ReplaceError(err => $"Failed to place rectangle of size {rectangleSize}: {err}");
    }

    private Result<None> ValidateRectangleSize(Size rectangleSize)
    {
        return (rectangleSize.Width > 0 && rectangleSize.Height > 0)
            ? Result.Ok()
            : Result.Fail<None>("Rectangle size must have positive dimensions");
    }

    private Result<None> UpdateMinDimension(Size size)
    {
        var newMinDimension = Math.Min(size.Width, size.Height);
        if (newMinDimension < _minDimension)
        {
            _minDimension = newMinDimension;
            _spiralEnumerator = _pointsProvider.GetSpiralPoints(_minDimension).GetEnumerator();
        }
        return Result.Ok();
    }

    private Result<Rectangle> FindPlaceForRectangle(Size rectangleSize)
    {
        for (int attempts = 0; attempts < 10000; attempts++)
        {
            var pointResult = GetNextSpiralPoint();
            if (!pointResult.IsSuccess)
                return Result.Fail<Rectangle>($"Failed to generate spiral points: {pointResult.Error}");
            
            var candidateRectangle = CreateRectangleAtPoint(rectangleSize, pointResult.Value);

            if (IntersectsWithAny(candidateRectangle)) 
                continue;
            
            var compactedRectangle = CompactRectangleTowardsCenter(candidateRectangle);
            _placedRectangles.Add(compactedRectangle);
            return Result.Ok(compactedRectangle);
        }
        
        return Result.Fail<Rectangle>("Too many attempts to place rectangle. Try different settings.");
    }

    private Result<Point> GetNextSpiralPoint()
    {
        _spiralEnumerator ??= _pointsProvider.GetSpiralPoints(_minDimension).GetEnumerator();
        
        return _spiralEnumerator.MoveNext()
            ? Result.Ok(_spiralEnumerator.Current)
            : Result.Fail<Point>("Failed to generate more spiral points");
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
                var movedX = MoveRectangleX(compacted, direction, stepSize);
                var movedY = MoveRectangleY(compacted, direction, stepSize);
                
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

    private Rectangle MoveRectangleX(Rectangle rectangle, Point direction, int stepSize)
    {
        return new Rectangle(
            rectangle.X + Math.Sign(direction.X) * stepSize,
            rectangle.Y,
            rectangle.Width,
            rectangle.Height);
    }

    private Rectangle MoveRectangleY(Rectangle rectangle, Point direction, int stepSize)
    {
        return new Rectangle(
            rectangle.X,
            rectangle.Y + Math.Sign(direction.Y) * stepSize,
            rectangle.Width,
            rectangle.Height);
    }

    private bool IntersectsWithAny(Rectangle rectangle)
    {
        return _placedRectangles.Any(placed => rectangle.IntersectsWith(placed));
    }
}