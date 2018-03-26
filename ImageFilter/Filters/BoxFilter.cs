using System.Drawing;

namespace ImageFilter.Filters
{
    public class BoxFilter : IPictureProcessor
    {
        private readonly int maskSize;
        private Bitmap processPicture;

        public BoxFilter(int maskSize)
        {
            this.maskSize = maskSize;
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var image = (Bitmap) loader.Image;

            var transform = new Tranformation();
            double[,] mask = transform.CreateBoxBlurFilter(maskSize);

            processPicture = transform.ProcessMask(image, mask, false);
            return processPicture;
        }
    }
}