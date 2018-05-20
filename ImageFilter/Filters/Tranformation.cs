﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using ImageFilter.Extensions;

namespace ImageFilter
{
    public class Tranformation
    {
        private readonly double stdDev = 1.4;

        public Tranformation()
        {
        }

        public Tranformation(double stdDev)
        {
            this.stdDev = stdDev;
        }

        private int Threshold { get; set; }
        private double Divider { get; set; }
        private bool UseDynamicDividerForEdges { get; } = true;

        public double[,] CreateLaplacianFilter(double alpha)
        {
            if (alpha == 0)
            {
                return new double[,] {
                    { 0, -1, 0 },
                    { -1, 4, -1 }, 
                    { 0, -1, 0 }
                };
            }
            else
            {
                return new double[,] { 
                    { 0, -1, 0 },
                    { -1, alpha + 4, -1 },
                    { 0, -1, 0 }
                };
            }
        }

        public double[,] CreateBoxBlurFilter(int maskSize)
        {
            var mask = new double[maskSize, maskSize];

            double divider = 0;

            for (var i = 0; i < maskSize; i++)
            {
                for (var j = 0; j < maskSize; j++)
                {
                    mask[i, j] = 1;
                    divider++;
                }
            }

            Divider = divider;
            return mask;
        }

        public double[,] CreateGaussFilter(int maskSize)
        {
            int maskLength = 2 * maskSize + 1;
            var mask = new double[maskLength, maskLength];
            double z = 0.0;
            for (int i = 0; i < maskLength; i++)
            {
                for (int j = 0; j < maskLength; j++)
                {
                    int x = i - maskSize;
                    int y = j - maskSize;
                    mask[i, j] = Math.Exp(-(Math.Pow(x, 2) + Math.Pow(y, 2)) / (2.0 * Math.Pow(stdDev, 2)));
                    z += mask[i, j];
                }
            }

            Divider = z;

            return mask;
        }

        public Bitmap ProcessMask(Bitmap src, double[,] mask, bool fixGamma)
        {
            int width = src.Width;
            int height = src.Height;
            var dest = new Bitmap(width, height, src.PixelFormat);

            using (var srcBMP = new ConcurrentBitmap(src))
            {
                using (var destBMP = new ConcurrentBitmap(dest))
                {
                    int maskLength = mask.GetLength(0);
                    int radius = maskLength >> 1;
                    int maskSize = maskLength * maskLength;
                    int threshold = Threshold;

                    // For each line
                    Parallel.For(
                        0,
                        height,
                        y =>
                        {
                            // For each pixel
                            for (var x = 0; x < width; x++)
                            {
                                // The number of mask elements taken into account
                                int processedSize;

                                // Color sums
                                double blue;
                                double divider;
                                double green;
                                double red = green = blue = divider = processedSize = 0;

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

                                    // Outwith the current bounds so break.
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

                                        double k = mask[i, j];
                                        divider += k;

                                        red += k * sourceColor.R;
                                        green += k * sourceColor.G;
                                        blue += k * sourceColor.B;

                                        processedSize++;
                                    }
                                }

                                // Check to see if all kernel elements were processed
                                if (processedSize == maskSize)
                                {
                                    // All kernel elements are processed; we are not on the edge.
                                    divider = Divider;
                                }
                                else
                                {
                                    // We are on an edge; do we need to use dynamic divider or not?
                                    if (!UseDynamicDividerForEdges)
                                    {
                                        // Apply the set divider.
                                        divider = Divider;
                                    }
                                }

                                // Check and apply the divider
                                if ((long) divider != 0)
                                {
                                    red /= divider;
                                    green /= divider;
                                    blue /= divider;
                                }

                                // Add any applicable threshold.
                                red += threshold;
                                green += threshold;
                                blue += threshold;


                                Color destinationColor = Color.FromArgb(
                                    Convert.ToByte(red.Clamp(0, 255)),
                                    Convert.ToByte(green.Clamp(0, 255)),
                                    Convert.ToByte(blue.Clamp(0, 255)));
                                
                                // ReSharper disable once AccessToDisposedClosure
                                destBMP.SetPixel(x, y, destinationColor);
                            }
                        }
                    );
                }
            }


            return dest;
        }

        public double[,] ProcessMaskDouble(Bitmap src, double[,] mask, bool fixGamma)
        {
            int width = src.Width;
            int height = src.Height;
            var dest = new double[height, width];

            using (var srcBMP = new ConcurrentBitmap(src))
            {
                int maskLength = mask.GetLength(0);
                int radius = maskLength >> 1;
                int maskSize = maskLength * maskLength;
                int threshold = Threshold;

                // For each line
                Parallel.For(
                    0,
                    height,
                    y =>
                    {
                        // For each pixel
                        for (var x = 0; x < width; x++)
                        {
                            // The number of mask elements taken into account
                            int processedSize;

                            // Color sums
                            double blue;
                            double divider;
                            double green;
                            double red = green = blue = divider = processedSize = 0;

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

                                // Outwith the current bounds so break.
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

                                    double k = mask[i, j];
                                    divider += k;

                                    red += k * sourceColor.R;
                                    green += k * sourceColor.G;
                                    blue += k * sourceColor.B;

                                    processedSize++;
                                }
                            }

                            // Check to see if all kernel elements were processed
                            if (processedSize == maskSize)
                            {
                                // All kernel elements are processed; we are not on the edge.
                                divider = Divider;
                            }
                            else
                            {
                                // We are on an edge; do we need to use dynamic divider or not?
                                if (!UseDynamicDividerForEdges)
                                {
                                    // Apply the set divider.
                                    divider = Divider;
                                }
                            }

                            // Check and apply the divider
                            if ((long)divider != 0)
                            {
                                red /= divider;
                                green /= divider;
                                blue /= divider;
                            }
                            dest[y, x] = red;
                        }
                    }
                );                
            }


            return dest;
        }

    }
}
