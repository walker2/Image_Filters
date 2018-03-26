using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using ImageFilter.Extensions;
using ImageFilter.Noises;

namespace ImageFilter
{
    public class ImageLoader : IDisposable
    {
        #region Fields

        private bool isDisposed = false;

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
            using (var img1BMP = new FastBitmap(img1))
            {
                using (var img2BMP = new FastBitmap(img2))
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
                            var I = (int) (  tmpColor.R * 0.299
                                           + tmpColor.G * 0.587
                                           + tmpColor.B * 0.114);

                            Color img2Color = Color.FromArgb(I, I, I);
                            
                            psnrY += Math.Pow(img1Color.R - img2Color.R, 2);
                        }
                    }
                }
            }

            psnrY = 10 * Math.Log10((width * height * Math.Pow((Math.Pow(2, 8) - 1), 2)) / psnrY);
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

            this.ImagePath = filePath;

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

        public ImageLoader AddNoise(INoise noise)
        {
            this.Image = noise.ProcessPicture(this);

            return this;
        }

        #region Dispose

        ~ImageLoader()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);

            // Already cleaned up in dispose method, supress GC
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;

            if (disposing)
            {
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
            }

            this.isDisposed = true;
        }

        #endregion

        #endregion
    }
}