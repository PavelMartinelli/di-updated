using System.Drawing;
using System.Drawing.Imaging;

namespace TagCloudGenerator;

public interface IImageSaver
{
    Result<string> SaveBitmap(Bitmap bitmap, string fileName, ImageFormat format = null);
}