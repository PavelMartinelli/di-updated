using System.Drawing;
using System.Drawing.Imaging;

namespace TagCloudGenerator;

public interface IImageSaver
{
    string SaveBitmap(Bitmap bitmap, string fileName, ImageFormat format = null);
}