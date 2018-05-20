﻿using System;
using System.Drawing;
using ImageFilter.Extensions;

namespace ImageFilter.Filters
{
    public class SobelFilter : IPictureProcessor
    {
        double[,] maskV = {
            { 1, 2, 1 },
            { 0, 0, 0 }, 
            { -1, -2, -1 }
        };
        double[,] maskH = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        private int v;
        private Bitmap processPicture;

        public SobelFilter(int v)
        {
            this.v = v;
        }

        public double[,] Get(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            var transform = new Tranformation();
            double[,] mask = v == 1 ? maskV : maskH;
            
            return transform.ProcessMaskDouble(image, mask, false);
        }

        public Bitmap ProcessPicture(ImageLoader loader)
        {
            var image = (Bitmap)loader.Image;

            var transform = new Tranformation();
            double[,] mask = v == 1 ? maskV : maskH;

            processPicture = transform.ProcessMask(image, mask, false);
            return processPicture;
        }

        public static double[,] CalculateGradient(double[,] Gv, double[,] Gh)
        {
            int height = Gv.GetLength(0);
            int width = Gv.GetLength(1);
            double[,] result = new double[height, width];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    double gv = Gv[y, x];
                    double gh = Gh[y, x];
                    var res = Math.Sqrt(Math.Pow(gv, 2) + Math.Pow(gh, 2));

                    result[y, x] = res;
                }
            }

            return result;
        }

        public static double[,] CalculateGradientDirection(double[,] Gv, double[,] Gh)
        {
            int height = Gv.GetLength(0);
            int width = Gv.GetLength(1);
            double[,] result = new double[height, width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    double gv = Gv[y, x];
                    double gh = Gh[y, x];
                    var res = Math.Atan2(gv, gh) * (180.0 / Math.PI);
                    
                    result[y, x] = res;
                }
            }

            return result;
        }

        public static Bitmap GetColorMap(double[,] Gv, double[,] Gh)
        {
            Bitmap result = new Bitmap(Gv.GetLength(0), Gv.GetLength(1));
            var direction = CalculateGradientDirection(Gv, Gh);

            for (var y = 0; y < direction.GetLength(0); y++)
            {
                for (var x = 0; x < direction.GetLength(1); x++)
                {
                    double blue;
                    double green;
                    double red = green = blue = 0;

                    if (direction[y, x] <= -90 && direction[y, x] >= -180)
                    {
                        // Red
                        red = 255;
                    } else if (direction[y, x] <= 0 && direction[y, x] > -90)
                    {
                        // Green
                        green = 255;
                    } else if (direction[y, x] <= 90 && direction[y, x] > 0)
                    {
                        // Blue
                        blue = 255;
                    } else
                    {
                        red = 255;
                        green = 255;
                        blue = 255;
                    }

                    Color destinationColor = Color.FromArgb(
                                    Convert.ToByte(red.Clamp(0, 255)),
                                    Convert.ToByte(green.Clamp(0, 255)),
                                    Convert.ToByte(blue.Clamp(0, 255)));

                    result.SetPixel(x, y, destinationColor);
                }
            }

            return result;
        }

        public static Bitmap GetBinaryCard(int thr, double[,] Gv, double[,] Gh)
        {
            Bitmap result = new Bitmap(Gv.GetLength(0), Gv.GetLength(1));
            var gradient = CalculateGradient(Gv, Gh);

            for (var y = 0; y < gradient.GetLength(0); y++)
            {
                for (var x = 0; x < gradient.GetLength(1); x++)
                {
                    double blue;
                    double green;
                    double red = green = blue = 0;

                    if (gradient[y, x] > thr)
                    {
                        red = green = blue = 255;
                    } else
                    {
                        red = green = blue = 0;
                    }

                    Color destinationColor = Color.FromArgb(
                                    Convert.ToByte(red.Clamp(0, 255)),
                                    Convert.ToByte(green.Clamp(0, 255)),
                                    Convert.ToByte(blue.Clamp(0, 255)));

                    result.SetPixel(x, y, destinationColor);
                }
            }

            return result;
        }

    }
}