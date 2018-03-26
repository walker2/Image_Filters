using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ImageFilter.Extensions;
using MathNet.Numerics.Random;

namespace ImageFilter.Noises
{
    public class ImpulseNoise : INoise
    {
        private double pA;
        private double pB;

        public ImpulseNoise(double pa, double pb)
        {
            pA = pa;
            pB = pb;
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var src = (Bitmap)loader.Image;
            int width = src.Width;
            int height = src.Height;
            var dest = new Bitmap(width, height, src.PixelFormat);

            System.Random rng = SystemRandomSource.Default;

            using (var srcBMP = new FastBitmap(src))
            {
                using (var newBMP = new FastBitmap(dest))
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
                                double sample = rng.NextDouble();
                                Color currentColor = srcBMP.GetPixel(x, y);
                                var I = (int)(currentColor.R * 0.299
                                              + currentColor.G * 0.587
                                              + currentColor.B * 0.114);

                                int res = I;
                                if (sample < pA)
                                {
                                    // PA
                                    res = byte.MinValue;
                                }
                                else if (sample < pA + pB)
                                {
                                    // PB
                                    res = byte.MaxValue;
                                }

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
