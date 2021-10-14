using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ScreenShotTool
{
    /// <summary>
    /// 屏幕图像转换帮助类
    /// </summary>
    public static class ScreenShotHelper
    {
        public static Bitmap GetScreenSnapshot()
        {
            Rectangle rc = SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        //public static BitmapSource ToBitmapSource(this Bitmap bmp)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        bmp.Save(stream, ImageFormat.Bmp);

        //        stream.Position = 0;
        //        BitmapImage result = new BitmapImage();
        //        result.BeginInit();
        //        // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
        //        // Force the bitmap to load right now so we can dispose the stream.
        //        result.CacheOption = BitmapCacheOption.OnLoad;
        //        result.StreamSource = stream;
        //        result.EndInit();
        //        result.Freeze();

        //        return result;
        //    }
        //}

        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height,
                                                   bitmap.HorizontalResolution, bitmap.VerticalResolution,
                                                   System.Windows.Media.PixelFormats.Bgra32, null,
                                                   bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
    }
}
