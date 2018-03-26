using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilter
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32 : IEquatable<Color32>
    {
        [FieldOffset(0)] public byte B;

        [FieldOffset(1)] public byte G;

        [FieldOffset(2)] public byte R;

        #region Equatable

        public override bool Equals(object obj)
        {
            if (obj is Color32)
            {
                return Equals((Color32) obj);
            }

            return false;
        }

        public bool Equals(Color32 other)
        {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        private int GetHashCode(Color32 obj)
        {
            unchecked
            {
                int hashCode = obj.B.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.G.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.R.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }

    public unsafe class ConcurrentBitmap : IDisposable
    {
        #region Fields

        private readonly Bitmap bitmap;

        private readonly int height;

        private readonly int width;

        private BitmapData bitmapData;

        private int bytesInARow;

        private bool isDisposed;

        private byte* pixelBase;

        private int pixelSize;

        #endregion


        public ConcurrentBitmap(Image bitmap)
        {
            var pixelFormat = (int) bitmap.PixelFormat;

            // Check image format
            if (pixelFormat != (int) PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Only 24bpp images are supported.");
            }

            this.bitmap = (Bitmap) bitmap;
            width = this.bitmap.Width;
            height = this.bitmap.Height;

            LockBitmap();
        }

        private Color32* this[int x, int y] => (Color32*) (pixelBase + y * bytesInARow + x * 3);


        public static implicit operator Image(ConcurrentBitmap concurrentBitmap)
        {
            return concurrentBitmap.bitmap;
        }

        public static implicit operator Bitmap(ConcurrentBitmap concurrentBitmap)
        {
            return concurrentBitmap.bitmap;
        }

        public Color GetPixel(int x, int y)
        {
#if DEBUG
            if (x < 0 || x >= width)
            {
                throw new ArgumentOutOfRangeException("x",
                    "Value cannot be less than zero or greater than the bitmap width.");
            }

            if (y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("y",
                    "Value cannot be less than zero or greater than the bitmap height.");
            }
#endif
            Color32* data = this[x, y];
            return Color.FromArgb(data->R, data->G, data->B);
        }

        public void SetPixel(int x, int y, Color color)
        {
#if DEBUG
            if (x < 0 || x >= width)
            {
                throw new ArgumentOutOfRangeException("x",
                    "Value cannot be less than zero or greater than the bitmap width.");
            }

            if (y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("y",
                    "Value cannot be less than zero or greater than the bitmap height.");
            }
#endif
            Color32* data = this[x, y];
            data->R = color.R;
            data->G = color.G;
            data->B = color.B;
        }

        public override bool Equals(object obj)
        {
            var fastBitmap = obj as ConcurrentBitmap;

            if (fastBitmap == null)
            {
                return false;
            }

            return bitmap == fastBitmap.bitmap;
        }

        public override int GetHashCode()
        {
            return bitmap.GetHashCode();
        }
        
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose of any managed resources here.
                UnlockBitmap();
            }

            // Note disposing is done.
            isDisposed = true;
        }

        private void LockBitmap()
        {
            var bounds = new Rectangle(Point.Empty, bitmap.Size);

            // Figure out the number of bytes in a row. This is rounded up to be a multiple
            // of 4 bytes, since a scan line in an image must always be a multiple of 4 bytes
            // in length.
            pixelSize = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            bytesInARow = bounds.Width * pixelSize;
            if (bytesInARow % 4 != 0)
            {
                bytesInARow = 4 * (bytesInARow / 4 + 1);
            }

            // Lock the bitmap
            bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // Set the value to the first scan line
            pixelBase = (byte*) bitmapData.Scan0.ToPointer();
        }

        private void UnlockBitmap()
        {
            // Copy the RGB values back to the bitmap and unlock the bitmap.
            bitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pixelBase = null;
        }
    }
}