using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ImageFilter.Extensions;

namespace ImageFilter.Filters
{
    class MedianFilter : IPictureProcessor
    {
        private readonly int radius;
        private Bitmap processPicture;

        public MedianFilter(int radius)
        {
            this.radius = radius;
        }

        private static int CompareColors(Color x, Color y)
        {
            return x.R.CompareTo(y.R);
        }
        
        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            int width = image.Width;
            int height = image.Height;
            var dest = new Bitmap(width, height, image.PixelFormat);

            using (var srcBMP = new ConcurrentBitmap(image))
            {
                using (var destBMP = new ConcurrentBitmap(dest))
                {
                    int maskLength = radius * 2 + 1;
                    int maskSize = maskLength * maskLength;
                    // For each line
                    Parallel.For(
                        0,
                        height,
                        y =>
                        {
                            for (var x = 0; x < width; x++)
                            {
                                // The number of mask elements taken into account
                                int processedSize;
                                 
                                double blue;
                                double divider;
                                double green;
                                double red = green = blue = divider = processedSize = 0;

                                var pixels = new List<Color>();
                                // For each kernel row
                                for (var i = 0; i < maskLength; i++)
                                {
                                    int ir = i - radius;
                                    int offsetY = y + ir;

                                    // Skip the current row
                                    if (offsetY < 0)
                                    {
                                        continue;
                                    }
                                    
                                    if (offsetY >= height)
                                    {
                                        break;
                                    }

                                    // For each kernel column
                                    for (var j = 0; j < maskLength; j++)
                                    {
                                        int jr = j - radius;
                                        int offsetX = x + jr;

                                        // Skip the column
                                        if (offsetX < 0 || offsetX >= width)
                                        {
                                            continue;
                                        }

                                        // ReSharper disable once AccessToDisposedClosure
                                        Color sourceColor = srcBMP.GetPixel(offsetX, offsetY);
                                        pixels.Add(sourceColor);

                                        processedSize++;
                                    }
                                }

                                pixels.Sort(CompareColors);

                                // Check and apply the divider
                                if ((long)divider != 0)
                                {
                                    red /= divider;
                                    green /= divider;
                                    blue /= divider;
                                }
                                int index = (int)(Math.Floor(Math.Pow(2 * radius + 1, 2) / 2) + 1);

                                Color destinationColor = pixels[pixels.Count / 2];
                                
                                // ReSharper disable once AccessToDisposedClosure
                                destBMP.SetPixel(x, y, destinationColor);
                            }
                        }
                    );
                }
            }


            return dest;
        }
    }
}
