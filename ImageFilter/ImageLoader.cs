﻿using System;
using System.Drawing;
using System.IO;
using ImageFilter.Filters;
using ImageFilter.Noises;
using ImageFilter.Extensions;
using OxyPlot;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;

namespace ImageFilter
{
    public class ImageLoader : IDisposable
    {
        #region Fields

        private bool isDisposed;

        #endregion

        #region Propeties

        public Image Image { get; set; }
        public string ImagePath { get; private set; }

        #endregion

        #region Methods

        public double CalculatePSNR(Image image)
        {
            var img1 = (Bitmap) Image;
            var img2 = (Bitmap) image;

            int width = image.Width;
            int height = image.Height;

            double psnrY = 0;
            using (var img1BMP = new ConcurrentBitmap(img1))
            {
                using (var img2BMP = new ConcurrentBitmap(img2))
                {
                    // For each line
                    for (var y = 0; y < height; y++)
                    {
                        // For each pixel
                        for (var x = 0; x < width; x++)
                        {
                            Color img1Color = img1BMP.GetPixel(x, y);

                            // Assumes that img2 is not in Y component
                            Color tmpColor = img2BMP.GetPixel(x, y);
                            var I = (int) (tmpColor.R * 0.299
                                           + tmpColor.G * 0.587
                                           + tmpColor.B * 0.114);

                            Color img2Color = Color.FromArgb(I, I, I);

                            psnrY += Math.Pow(img1Color.R - img2Color.R, 2);
                        }
                    }
                }
            }

            psnrY = 10 * Math.Log10(width * height * Math.Pow(Math.Pow(2, 8) - 1, 2) / psnrY);
            /*Console.WriteLine($"Y: {psnrY}");*/
            return psnrY;
        }

        public ImageLoader Load(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filePath);
            }

            ImagePath = filePath;

            // Open a file stream

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var stream = new MemoryStream();
                fileStream.CopyTo(stream);
                stream.Position = 0;

                Image = Image.FromStream(stream);
            }

            return this;
        }

        

        public ImageLoader Save(string filePath)
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            Image.Save(filePath);

            return this;
        }

        public ImageLoader AddImage(Image img)
        {
            Bitmap one = (Bitmap) Image;

            Bitmap two = (Bitmap) img;

            int height = Image.Height;
            int width =  Image.Width;
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    double blue;
                    double green;
                    double red = green = blue = 0;

                    Color colorOne = one.GetPixel(x, y);
                    Color colorTwo = two.GetPixel(x, y);

                    red = colorOne.R + colorTwo.R;
                    green = colorOne.G + colorTwo.G;
                    blue = colorOne.B + colorTwo.B;

                    Color destinationColor = Color.FromArgb(
                                    Convert.ToByte(red.Clamp(0, 255)),
                                    Convert.ToByte(green.Clamp(0, 255)),
                                    Convert.ToByte(blue.Clamp(0, 255)));

                    one.SetPixel(x, y, destinationColor);
                }
            }


            return this;
        }

        public ImageLoader Add128()
        {
            Bitmap image = (Bitmap) Image;
            int height = image.Height;
            int width = image.Width;
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    double blue;
                    double green;
                    double red = green = blue = 0;

                    Color color = image.GetPixel(x, y);
                    red = color.R + 128;
                    green = color.G + 128;
                    blue = color.B + 128;

                    Color destinationColor = Color.FromArgb(
                                    Convert.ToByte(red.Clamp(0, 255)),
                                    Convert.ToByte(green.Clamp(0, 255)),
                                    Convert.ToByte(blue.Clamp(0, 255)));

                    image.SetPixel(x, y, destinationColor);
                }
            }

            return this;
        }

        public ImageLoader AddNoise(INoise noise)
        {
            Image = noise.ProcessPicture(this);

            return this;
        }

        public ImageLoader AddBoxFilter(int size)
        {
            var boxFilter = new BoxFilter(size);
            Image = boxFilter.ProcessPicture(this);

            return this;
        }

        public ImageLoader AddGaussFilter(int size, double sigma)
        {
            var boxFilter = new GaussFilter(size, sigma);
            Image = boxFilter.ProcessPicture(this);

            return this;
        }

        public ImageLoader AddMedianFilter(int size)
        {
            var medianFilter = new MedianFilter(size);
            Image = medianFilter.ProcessPicture(this);

            return this;
        }

        public ImageLoader AddLaplacianFilter(double alpha)
        {
            var laplacianFilter = new LaplacianFilter(alpha);
            Image = laplacianFilter.ProcessPicture(this);

            return this;
        }

        public ImageLoader AddSobelFilter(string direction)
        {
            var sobelFilter = new SobelFilter(direction == "V" ? 1 : 0);

            Image = sobelFilter.ProcessPicture(this);

            return this;
        }

        public double[,] GetSobelFilterDouble(string direction)
        {
            var sobelFilter = new SobelFilter(direction == "V" ? 1 : 0);

            return sobelFilter.Get(this);
        }

        public double CalculateAvgLuminocity()
        {
            double luminocity = 0;
            Bitmap image = (Bitmap)Image;
            int height = image.Height;
            int width = image.Width;
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    luminocity += image.GetPixel(x, y).R;
                }
            }

            return luminocity / (double) (height * width);
        }

        private SortedDictionary<int, int> GetFrequency()
        {
            Bitmap image = (Bitmap)Image;
            int height = image.Height;
            int width = image.Width;

            var result = new SortedDictionary<int, int>();
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    int Y = image.GetPixel(x, y).R;

                    if (result.ContainsKey(Y))
                    {
                        result[Y] = result[Y] + 1;
                    } else
                    {
                        result.Add(Y, 1);
                    }
                }
            }
            return result;
        }

        public void PlotHistogram(string name, string outputPath, FileInfo fileInfo)
        {
            PngExporter pngExporter = new PngExporter { Width = 1280, Height = 720, Background = OxyColors.White };

            var freq = GetFrequency();

            var x = freq.Values;
            var y = freq.Keys;

            var plotGauss = new PlotModel { Title = $"Histogram for {name}" };
            plotGauss.Axes.Add(new LinearAxis { Title = "x", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "y", Position = AxisPosition.Left });

            var barSeries = new LinearBarSeries
            {

            };

            foreach (var item in freq)
            {
                barSeries.Points.Add(new DataPoint(item.Key, item.Value));
            }

            plotGauss.Series.Add(barSeries);
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/histograms/{name}{fileInfo.Extension}");
        }

        #region Dispose

        ~ImageLoader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            // Already cleaned up in dispose method, supress GC
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (Image != null)
                {
                    Image.Dispose();
                    Image = null;
                }
            }

            isDisposed = true;
        }

        #endregion

        #endregion
    }
}