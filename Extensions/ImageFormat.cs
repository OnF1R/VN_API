using System.Text;

namespace VN_API.Extensions
{
    public enum ImageType
    {
        bmp,
        jpg,
        gif,
        tiff,
        png,
        unknown
    }

    public static class ImageFormat
    {
        public static ImageType GetImageFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var jpg = new byte[] { 255, 216, 255, 219 }; // jpg 

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageType.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageType.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageType.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageType.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageType.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageType.jpg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageType.jpg;

            if (jpg.SequenceEqual(bytes.Take(jpg.Length)))
                return ImageType.jpg;

            return ImageType.unknown;
        }

    }
}
