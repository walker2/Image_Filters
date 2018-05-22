using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageFilter.Noises;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Interpolation;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.WindowsForms;
using LineSeries = OxyPlot.Series.LineSeries;
using ImageFilter.Extensions;

namespace ImageFilter
{
    internal class Program
    {
        private static readonly string OutputPath = TestContext.CurrentContext.TestDirectory + @"/output/";

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var original = TestImages.GetTestImagesFromTestFolder("");

            foreach (FileInfo file in original)
            {
                string outputFileName = $"{OutputPath}{file.Name.Substring(0, file.Name.LastIndexOf('.'))}";
                Console.WriteLine(file.Name);

                /* NOISES */
                //Noise(file);

                /* BOXFILTER */
                //BoxFilter(file, outputFileName);

                /* GAUSSFILTER */
                //GaussFilter(file, outputFileName);

                /* MEDIANFILTER */
                //MedianFilter(file, outputFileName);

                /* LAPLACIANFILTER */
                LaplacianFilter(file, outputFileName);

                /* SOBELFILTER */
                SobelFilter(file, outputFileName);

                //var plotBuilder = new PlotBuilder(OutputPath, file);
                //plotBuilder.PlotAll();

                // Just run first image
                break;
                
            }

            var modified = TestImages.GetTestImagesFromTestFolder("/LowHigh/");
            foreach (FileInfo file in modified)
            {
                string outputFileName = $"{OutputPath}{file.Name.Substring(0, file.Name.LastIndexOf('.'))}";
                Console.WriteLine(file.Name);
                /* TWODOTGRAD */
                TwoDotGrad(file, outputFileName);
            }

            timer.Stop();
            Console.WriteLine("Runtime: " + timer.ElapsedMilliseconds / 1000.0 + " s");
        }
        

        private static double LinearInterpolation(double x, double x0, double x1, double y0, double y1)
        {
            if ((x1 - x0) == 0)
            {
                return (y0 + y1) / 2;
            }
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

        private static void TwoDotGrad(FileInfo file, string outputFileName)
        {
            double[] a = { 0, 65, 180, 255 };
            double[] b = { 0, 40, 210, 255 };

            var f = MathNet.Numerics.Interpolate.Linear(a, b);
            
            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                imageLoader.AddNoise(new GaussNoise(new Normal(0.0001, 0.0001)));
                Bitmap image = (Bitmap) imageLoader.Image;

                imageLoader.PlotHistogram("before", OutputPath, file);
                int height = image.Height;
                int width = image.Width;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        double color = f.Interpolate(image.GetPixel(x, y).R);
                        Color destinationColor = Color.FromArgb(
                                        Convert.ToByte(color.Clamp(0, 255)),
                                        Convert.ToByte(color.Clamp(0, 255)),
                                        Convert.ToByte(color.Clamp(0, 255)));

                        image.SetPixel(x, y, destinationColor);
                    }
                }

                imageLoader.Image = image;
                imageLoader.Save($"{outputFileName}_gradation_{file.Extension}");
                imageLoader.PlotHistogram("after", OutputPath, file);
            }
                
        }

        private static void SobelFilter(FileInfo file, string outputFileName)
        {
            Console.WriteLine("***SOBEL FILTER***");

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                imageLoader.AddNoise(new GaussNoise(new Normal(0.0001, 0.0001)));
                Image I = imageLoader.Image;

                //Image Gh = imageLoader.AddSobelFilter("H").Image;
                //imageLoader.Save($"{outputFileName}_GH_sobel{file.Extension}");
                //imageLoader.Image = I;
                //Image Gv = imageLoader.AddSobelFilter("V").Image;
                //imageLoader.Save($"{outputFileName}_GV_sobel{file.Extension}");

                double[,] Gv = imageLoader.GetSobelFilterDouble("V");
                double[,] Gh = imageLoader.GetSobelFilterDouble("H");

                Image ColorMap = Filters.SobelFilter.GetColorMap(Gv, Gh);

                imageLoader.Image = ColorMap;
                imageLoader.Save($"{outputFileName}_sobel_ColorMap_{file.Extension}");

                for (int thr = 15; thr < 200; thr += 10)
                {
                    Image BinaryCard = Filters.SobelFilter.GetBinaryCard(thr, Gv, Gh);
                    imageLoader.Image = BinaryCard;
                    imageLoader.Save($"{outputFileName}_sobel_BinaryCard_(thr={thr}){file.Extension}");
                }
            }
        }

        private static void LaplacianFilter(FileInfo file, string outputFileName)
        {
            Console.WriteLine("***LAPLACIAN FILTER***");

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            double[] alphas = { 0, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6 };
            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                imageLoader.AddNoise(new GaussNoise(new Normal(0.0001, 0.0001)));

                Image I = imageLoader.Image;
                Console.WriteLine($"Avg luma of src: {imageLoader.CalculateAvgLuminocity()}");

                imageLoader.PlotHistogram("src", OutputPath, file);

                foreach (var alpha in alphas)
                {
                    if (alpha == 0)
                    {
                        double[,] I1 = imageLoader.AddLaplacianFilterAlpha0();
                        //imageLoader.Save($"{outputFileName}_I1_laplacianfiltered{file.Extension}");

                        //Console.WriteLine($"Avg luma of I1: {imageLoader.CalculateAvgLuminocity()}");
                        //imageLoader.PlotHistogram("I1", OutputPath, file);

                        imageLoader.Add128(I1);
                        imageLoader.Save($"{outputFileName}_I1+128_laplacianfiltered{file.Extension}");

                        Console.WriteLine($"Avg luma of I1 + 128: {imageLoader.CalculateAvgLuminocity()}");
                        imageLoader.PlotHistogram("I1+128", OutputPath, file);
                        imageLoader.Image = I;


                        imageLoader.AddImage(I, I1);
                        imageLoader.Save($"{outputFileName}_I2_laplacianfiltered{file.Extension}");

                        Console.WriteLine($"Avg luma of I2: {imageLoader.CalculateAvgLuminocity()}");
                        imageLoader.PlotHistogram("I2", OutputPath, file);
                    }
                    else
                    {
                        imageLoader.AddLaplacianFilter(alpha);
                        imageLoader.Save($"{outputFileName}laplacianfiltered_(alpha={alpha}){file.Extension}");

                        Console.WriteLine($"Avg luma of alpha = {alpha}: {imageLoader.CalculateAvgLuminocity()}");
                        imageLoader.PlotHistogram($"alpha={alpha}", OutputPath, file);
                    }

                    imageLoader.Image = I;
                }               
            }
        }

        private static void MedianFilter(FileInfo file, string outputFileName)
        {
            Console.WriteLine("***MEDIAN FILTER***");

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                Image image = imageLoader.Image;
                imageLoader.AddNoise(new ImpulseNoise(0.125, 0.125));
                imageLoader.Save($"{outputFileName}impulsenoise{file.Extension}");
                Console.WriteLine($"psnr-noise:     {imageLoader.CalculatePSNR(image):F2}");

                imageLoader.AddMedianFilter(2);
                imageLoader.Save($"{outputFileName}medianfiltered{file.Extension}");
                Console.WriteLine($"psnr-medianfilter: {imageLoader.CalculatePSNR(image):F2}");
            }
        }

        private static void GaussFilter(FileInfo file, string outputFileName)
        {
            Console.WriteLine("***GAUSS FILTER***");

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                Image image = imageLoader.Image;
                imageLoader.AddNoise(new GaussNoise(new Normal(0, 0.25)));
                imageLoader.Save($"{outputFileName}gaussnoise{file.Extension}");
                Console.WriteLine($"psnr-noise:     {imageLoader.CalculatePSNR(image):F2}");

                imageLoader.AddGaussFilter(4, 1.6);
                imageLoader.Save($"{outputFileName}gaussfiltered{file.Extension}");
                Console.WriteLine($"psnr-gaussfilter: {imageLoader.CalculatePSNR(image):F2}");
            }
        }

        private static void BoxFilter(FileInfo file, string outputFileName)
        {
            Console.WriteLine("***BOX FILTER***");

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(file.FullName);
                Image image = imageLoader.Image;
                imageLoader.AddNoise(new GaussNoise(new Normal(0, 0.25)));
                imageLoader.Save($"{outputFileName}noise{file.Extension}");
                Console.WriteLine($"psnr-noise:     {imageLoader.CalculatePSNR(image):F2}");

                imageLoader.AddBoxFilter(5);
                imageLoader.Save($"{outputFileName}boxfiltered{file.Extension}");
                Console.WriteLine($"psnr-boxfilter: {imageLoader.CalculatePSNR(image):F2}");
            }
        }

        private static void Noise(FileInfo file)
        {
            Console.WriteLine("***ADDING NOISES***");
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var gaussPoints = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 4,
                Color = OxyColors.Red
            };

            var impulsePoints = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 4,
                Color = OxyColors.Blue
            };

            var psnrGauss = new List<double>();
            var psnrImpulse = new List<double>();

            using (var imageLoader = new ImageLoader())
            {
                var stddevValues = new List<double>
                {
                    0.025,
                    0.05,
                    0.10,
                    0.25,
                    0.5
                };

                var probValuse = new List<double>
                {
                    0.025,
                    0.05,
                    0.1,
                    0.25,
                    0.5
                };

                imageLoader.Load(file.FullName);
                Image image = imageLoader.Image;

                foreach (double stddev in stddevValues)
                {
                    imageLoader.AddNoise(new GaussNoise(new Normal(0, stddev)));

                    double psnr = imageLoader.CalculatePSNR(image);

                    psnrGauss.Add(psnr);
                    gaussPoints.Points.Add(new DataPoint(stddev, psnr));

                    imageLoader.Image = image;
                }

                foreach (double prob in probValuse)
                {
                    imageLoader.AddNoise(new ImpulseNoise(prob, prob));
                    double psnr = imageLoader.CalculatePSNR(image);

                    psnrImpulse.Add(psnr);
                    impulsePoints.Points.Add(new DataPoint(prob, psnr));
                    imageLoader.Image = image;
                }
            }

            Console.WriteLine("Gauss noise: ");
            foreach (double psnr in psnrGauss)
            {
                Console.Write($"{psnr:F2}; ");
            }

            Console.WriteLine();

            Console.WriteLine("Impulse noise: ");
            foreach (double psnr in psnrImpulse)
            {
                Console.Write($"{psnr:F2}; ");
            }

            Console.WriteLine();

            var pngExporter = new PngExporter {Width = 1280, Height = 720, Background = OxyColors.White};

            var plotGauss = new PlotModel {Title = "Gauss noise plot"};
            plotGauss.Series.Add(gaussPoints);
            plotGauss.Series.Add(impulsePoints);
            pngExporter.ExportToFile(plotGauss, $"{OutputPath}/noise_psnr.png");
        }
    }
}