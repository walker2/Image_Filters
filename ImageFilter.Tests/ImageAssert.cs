using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ImageFilter.Tests
{
    class ImageAssert
    {
        /* Converts an image to a byte array */
        private static IEnumerable<byte> ToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        /* Asserts that two images are different */
        public static void AssertImagesAreDifferent(Image expected, Image tested)
        {
            Assert.IsFalse(ToByteArray(expected).SequenceEqual(ToByteArray(tested)));
        }
    }
}
