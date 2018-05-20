using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ImageFilter.Noises;
using MathNet.Numerics.Distributions;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ImageFilter
{
	public class PlotBuilder
    {

        private string outputPath;
        private FileInfo fileInfo;
        private PngExporter pngExporter = new PngExporter { Width = 1280, Height = 720, Background = OxyColors.White };

        public PlotBuilder(string outputPath, FileInfo fileInfo)
        {
            this.outputPath = outputPath;
            this.fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }


        public void PlotAll()
        {
            PlotAdditiveNoise();
            PlotImpulseNoise();
            PlotGaussR();
            PlotGaussAll();
            PlotMedianFilter();
        }


        private void PlotAdditiveNoise()
        {
            var plotGauss = new PlotModel { Title = $"Additive Noise Plot for {fileInfo.Name}" };
            plotGauss.Axes.Add(new LinearAxis { Title = "Standard Deviation", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            var gaussPoints = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 4,
                Color = OxyColors.Red
            };
            gaussPoints.MarkerType = MarkerType.Circle;

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;
                for (var stddev = 0.0; stddev < 240; stddev += 10)
                {
                    imageLoader.AddNoise(new GaussNoise(new Normal(0, stddev)));

                    double psnr = imageLoader.CalculatePSNR(image);

                    gaussPoints.Points.Add(new DataPoint(stddev, psnr));

                    imageLoader.Image = image;
                }
            }
            
            plotGauss.Series.Add(gaussPoints);
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/AdditiveNoisePlot.png");
        }

        private void PlotImpulseNoise()
        {
            var plotGauss = new PlotModel { Title = $"Impulse Noise Plot for {fileInfo.Name}" };

            plotGauss.Axes.Add(new LinearAxis { Title = "p2", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;

                for (var p1 = 0.1; p1 <= 0.5; p1 += 0.1)
                {
                    var impulsePoints = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 4,
                        Title = $"p1 = {p1}"
                    };
                    impulsePoints.MarkerType = MarkerType.Circle;

                    for (var p2 = 0.0; p2 <= 0.5; p2 += 0.1)
                    {
                        imageLoader.AddNoise(new ImpulseNoise(p1, p2));

                        double psnr = imageLoader.CalculatePSNR(image);
                        
                        impulsePoints.Points.Add(new DataPoint(p2, psnr));

                        imageLoader.Image = image;
                    }

                    plotGauss.Series.Add(impulsePoints);
                }
            }
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/ImpulseNoisePlot.png");
        }

        void PlotGaussAll()
        {
            var plotGauss = new PlotModel { Title = $"Gauss Filter Plot for {fileInfo.Name}" };

            plotGauss.Axes.Add(new LinearAxis { Title = "Sigma", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;
               // Image noise = imageLoader.AddNoise(new GaussNoise(new Normal(0, 0.25))).Image;

                for (var R = 1; R <= 8; R++)
                {
                    var points = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 4,
                        Title = $"R = {R}"
                    };
                    points.MarkerType = MarkerType.Circle;

                    for (var sigma = 0.5; sigma <= 3; sigma += 0.5)
                    {
                        imageLoader.AddNoise(new GaussNoise(new Normal(0, 100))); //dispersy -- 100

                        imageLoader.AddGaussFilter(R, sigma);

                        double psnr = imageLoader.CalculatePSNR(image);

                        points.Points.Add(new DataPoint(sigma, psnr));

                        imageLoader.Image = image;
                    }

                    plotGauss.Series.Add(points);
                }
            }
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/GaussFilterPlot.png");
        }

        void PlotGaussR()
        {
            var plotGauss = new PlotModel { Title = $"Gauss R with sigma 2 for {fileInfo.Name}" };

            plotGauss.Axes.Add(new LinearAxis { Title = "R", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;

                var points = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 4,
                    Color = OxyColors.Red
                };

                for (var R = 1; R <= 8; R++)
                {
                    //imageLoader.Image = noise;
                    imageLoader.AddNoise(new GaussNoise(new Normal(0, 100))); // 100 dispersy
					imageLoader.AddGaussFilter(R, 2);

					double psnr = imageLoader.CalculatePSNR(image);

					points.Points.Add(new DataPoint(R, psnr));

                    imageLoader.Image = image;

                }
                plotGauss.Series.Add(points);
            }
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/GaussFilterPlotR.png");

        }

        private void PlotGaussFilter()
        {
            var plotGauss = new PlotModel { Title = $"Gauss Filter Plot for {fileInfo.Name}" };

            plotGauss.Axes.Add(new LinearAxis { Title = "Sigma", Position = AxisPosition.Bottom });
            plotGauss.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;
                Image noise = imageLoader.AddNoise(new GaussNoise(new Normal(0, 0.25))).Image;

                for (var R = 1; R <= 8; R++)
                {
                    var points = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 4,
                        Title = $"R = {R}"
                    };
                    points.MarkerType = MarkerType.Circle;

                    for (var sigma = 0.5; sigma <= 3; sigma += 0.5)
                    {
                        imageLoader.Image = noise;

                        imageLoader.AddGaussFilter(R, sigma);

                        double psnr = imageLoader.CalculatePSNR(image);

                        points.Points.Add(new DataPoint(sigma, psnr));

                        imageLoader.Image = image;
                    }

                    plotGauss.Series.Add(points);
                }
            }
            pngExporter.ExportToFile(plotGauss, $"{outputPath}/{fileInfo.Name}/GaussFilterPlot.png");
        }

        private void PlotMedianFilter()
        {
            var plotMedian = new PlotModel { Title = $"Median Filter Plot for {fileInfo.Name}" };

            plotMedian.Axes.Add(new LinearAxis { Title = "R", Position = AxisPosition.Bottom });
            plotMedian.Axes.Add(new LinearAxis { Title = "PSNR", Position = AxisPosition.Left });

            var probValue = new List<double>
                {
                    0.025,
                    0.050,
                    0.125,
                    0.250
                };

            using (var imageLoader = new ImageLoader())
            {
                imageLoader.Load(fileInfo.FullName);
                Image image = imageLoader.Image;

                foreach (var value in probValue)
                {
                    var points = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 4,
                        Title = $"{value * 2 * 100} %"
                    };
                    points.MarkerType = MarkerType.Circle;

                    for (var R = 1; R <= 5; R++)
                    {
                        imageLoader.AddNoise(new ImpulseNoise(value, value));
                        imageLoader.AddMedianFilter(R);

                        double psnr = imageLoader.CalculatePSNR(image);

                        points.Points.Add(new DataPoint(R, psnr));

                        imageLoader.Image = image;
                    }

                    plotMedian.Series.Add(points);
                }
            }
            pngExporter.ExportToFile(plotMedian, $"{outputPath}/{fileInfo.Name}/MedianFilterPlot.png");
        }
    }
}


