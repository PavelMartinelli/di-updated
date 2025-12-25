using System.Drawing;

namespace TagCloudGenerator;

public interface ICloudLayouter
{
    Result<Rectangle> PutNextRectangle(Size rectangleSize);
}