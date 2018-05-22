using System.Drawing;

namespace ImageFilter.Filters
{
    public class LaplacianFilter : IPictureProcessor
    {
        private double alpha = 0;
        private Bitmap processPicture;

        public LaplacianFilter()
        {

        }

        public LaplacianFilter(double alpha)
        {
            this.alpha = alpha;
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            var transform = new Tranformation();
            double[,] mask = transform.CreateLaplacianFilter(alpha);

            processPicture = transform.ProcessMask(image, mask, false);
            return processPicture;
        }

        public double[,] ProcessPictureAlpha0(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            var transform = new Tranformation();
            double[,] mask = transform.CreateLaplacianFilter(0);

            return transform.ProcessMaskDouble(image, mask, false);
        }
    }
}