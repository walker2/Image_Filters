using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using ImageFilter.Noises;
using MathNet.Numerics.Distributions;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace ImageFilter.Tests
{
    [TestFixture]
    public class ImageLoaderTests
    {
        private static readonly string OutputPath = TestContext.CurrentContext.TestDirectory + @"\output\";


        [Test]
        public void Image_Is_Loaded_From_File()
        {
            foreach (var file in TestImages.GetTestImagesFromTestFolder(""))
            {
                using (var imageLoader = new ImageLoader())
                {
                    imageLoader.Load(file.FullName);
                    Assert.AreEqual(imageLoader.ImagePath, file.FullName);
                    Assert.NotNull(imageLoader.Image);
                }
            }
        }

        [Test]
        public void Image_Is_Saved_To_File()
        {
            foreach (var file in TestImages.GetTestImagesFromTestFolder(""))
            {
                var outputFileName = $"{OutputPath}{file.Name}";
                using (var imageLoader = new ImageLoader())
                {
                    imageLoader.Load(file.FullName);
                    imageLoader.Save(outputFileName);

                    Assert.IsTrue(File.Exists(outputFileName));
                    File.Delete(outputFileName);
                }
            }
        }

        [Test]
        public void Noise_Is_Applied()
        {
            foreach (FileInfo file in TestImages.GetTestImagesFromTestFolder(""))
            {
                string outputFileName = $"{OutputPath}{file.Name.Substring(0, file.Name.LastIndexOf('.'))}";

                using (var imageLoader = new ImageLoader())
                {
                    var noises = new List<INoise>
                    {
                        new GaussNoise(new Normal(0, 0.25)),
                        new ImpulseNoise(0.025, 0.025)
                    };
                    imageLoader.Load(file.FullName);
                    Image image = imageLoader.Image;

                    foreach (INoise noise in noises)
                    {
                        string noiseFileName = $"{outputFileName}_{noise.GetType().Name.ToLower()}{file.Extension}";
                        imageLoader.AddNoise(noise);
                        imageLoader.Save(noiseFileName);
                        Assert.IsTrue(File.Exists(noiseFileName));
                        ImageAssert.AssertImagesAreDifferent(image, imageLoader.Image);
                        imageLoader.Image = image;
                    }

                    //File.Delete(outputFileName);
                }
            }
        }

        [Apartment(ApartmentState.STA)]
        [Test]
        public void TestGraph_Is_Created()
        {
            var plotModel = new PlotModel {Title = "Test Plot"};
            plotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            var pngExporter = new PngExporter {Width = 1280, Height = 720, Background = OxyColors.White};

            pngExporter.ExportToFile(plotModel, $"{OutputPath}/test.png");
            Assert.IsTrue(File.Exists($"{OutputPath}/test.png"));
        }

        [Test]
        public void BoxFilter_Is_Applied()
        {
            foreach (FileInfo file in TestImages.GetTestImagesFromTestFolder(""))
            {
                string outputFileName = $"{OutputPath}{file.Name.Substring(0, file.Name.LastIndexOf('.'))}";

                using (var imageLoader = new ImageLoader())
                {
                    imageLoader.Load(file.FullName);
                    Image image = imageLoader.Image;
                    string fileName = $"{outputFileName}_boxFilter{file.Extension}";

                    imageLoader.AddBoxFilter(5);
                    imageLoader.Save(fileName);

                    Assert.IsTrue(File.Exists(fileName));
                    ImageAssert.AssertImagesAreDifferent(image, imageLoader.Image);
                }
            }
        }

        [Test]
        public void GaussFilter_Is_Applied()
        {
            foreach (FileInfo file in TestImages.GetTestImagesFromTestFolder(""))
            {
                string outputFileName = $"{OutputPath}{file.Name.Substring(0, file.Name.LastIndexOf('.'))}";

                using (var imageLoader = new ImageLoader())
                {
                    imageLoader.Load(file.FullName);
                    Image image = imageLoader.Image;
                    string fileName = $"{outputFileName}_gaussFilter{file.Extension}";

                    imageLoader.AddGaussFilter(5, 1.4);
                    imageLoader.Save(fileName);

                    Assert.IsTrue(File.Exists(fileName));
                    ImageAssert.AssertImagesAreDifferent(image, imageLoader.Image);
                }
            }
        }
    }
}