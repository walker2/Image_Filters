using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFilter
{
    public interface IPictureProcessor
    {
        Bitmap ProcessPicture(ImageLoader loader);
    }
}
