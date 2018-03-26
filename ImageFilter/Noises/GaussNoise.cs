using System.Drawing;
using System.Threading.Tasks;
using ImageFilter.Extensions;
using ImageFilter.Noises;
using MathNet.Numerics.Distributions;

namespace ImageFilter
{
    public class GaussNoise : INoise
    {
        private readonly Normal distribution;

        public GaussNoise(Normal distibution)
        {
            distribution = distibution;
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var src = (Bitmap) loader.Image;

            int width = src.Width;
            int height = src.Height;
            var dest = new Bitmap(width, height, src.PixelFormat);

            using (var srcBMP = new ConcurrentBitmap(src))
            {
                using (var newBMP = new ConcurrentBitmap(dest))
                {
                    // For each line
                    Parallel.For(
                        0,
                        height,
                        y =>
                        {
                            // For each pixel
                            for (var x = 0; x < width; x++)
                            {
                                Color currentColor = srcBMP.GetPixel(x, y);

                                var I = (int) (currentColor.R * 0.299
                                               + currentColor.G * 0.587
                                               + currentColor.B * 0.114);

                                var res = (int) (I + distribution.Sample() * 128).Clamp(byte.MinValue, byte.MaxValue);

                                Color output = Color.FromArgb(res, res, res);

                                newBMP.SetPixel(x, y, output);
                            }
                        }
                    );
                }
            }

            //src.Dispose();
            return dest;
        }
    }
}