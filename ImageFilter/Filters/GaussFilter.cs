using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFilter.Filters
{
    class GaussFilter : IPictureProcessor
    {
        private readonly int maskSize;
        private readonly double sigma;

        private Bitmap processPicture;

        public GaussFilter(int maskSize, double sigma)
        {
            this.maskSize = maskSize;
            this.sigma = sigma;
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            var transform = new Tranformation(sigma);
            double[,] mask = transform.CreateGaussFilter(maskSize);

            processPicture = transform.ProcessMask(image, mask, false);
            return processPicture;
        }
    }
}
