using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageFilter.Noises;
using MathNet.Numerics.Distributions;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.WindowsForms;
using LineSeries = OxyPlot.Series.LineSeries;

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
            foreach (FileInfo file in TestImages.GetTestImagesFromTestFolder(""))
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


                var plotBuilder = new PlotBuilder(OutputPath, file);
                plotBuilder.PlotAll();

                // Just run first image
                break;
            }
            timer.Stop();
            Console.WriteLine("Runtime: " + timer.ElapsedMilliseconds / 1000.0 + " s");
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