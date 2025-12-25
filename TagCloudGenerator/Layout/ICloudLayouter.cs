namespace TagCloudGenerator;

using System.Drawing;

public interface ICloudLayouter
{
    Result<Rectangle> PutNextRectangle(Size rectangleSize);
}