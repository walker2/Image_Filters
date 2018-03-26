using System.Drawing;

namespace ImageFilter
{
    public interface IPictureProcessor
    {
        Bitmap ProcessPicture(ImageLoader loader);
    }
}