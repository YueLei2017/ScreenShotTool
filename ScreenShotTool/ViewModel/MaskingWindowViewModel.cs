using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShotTool
{
    public class MaskingWindowViewModel : BindableBase
    {
        public MaskingWindowViewModel()
        {
            screen = ScreenShotHelper.GetScreenSnapshot();
            ScreenSource = screen.ToBitmapSource();
        }
        public MaskingWindowView View { get; set; }
        public  Bitmap screen { get; set; }

        private ImageSource screenSource;
        public ImageSource ScreenSource
        {
            get { return screenSource; }
            set
            {
                SetProperty(ref screenSource, value);
            }
        }

        /// <summary>
        /// 键盘按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MaskingPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //按下了ESC键,退出截图
            if(e.Key == Key.Escape)
            {
                ExitScreenShot();
            }
        }
        public void MaskingUnloaded(object sender, RoutedEventArgs e)
        {
            //资源释放
            screen?.Dispose();
            screen = null;
            ScreenSource = null;
        }

        public void ExitScreenShot()
        {
            View.Close();
        }

        /// <summary>
        /// 用于保存截图
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public Bitmap CopyBitmapFromScreenSnapshot(Rect region)
        {
            if (region.Width.Equals(0.0) || region.Height.Equals(0.0))
            {
                return null;
            }
            var sourceRect = new Rectangle(Convert.ToInt32(region.X), Convert.ToInt32(region.Y),
                                           Convert.ToInt32(region.Width), Convert.ToInt32(region.Height));
            var destRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

            if (ScreenSource != null)
            {
                  Bitmap bitmap = new Bitmap(sourceRect.Width, sourceRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(screen, destRect, sourceRect, GraphicsUnit.Pixel);
                }
                return bitmap;
            }
            return null;
        }

        /// <summary>
        /// 用于放大镜截图
        /// </summary>
        /// <param name="region"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Bitmap CopyBitmapFromScreenSnapshot(Rect region, Bitmap bitmap)
        {
            if (region.Width.Equals(0.0) || region.Height.Equals(0.0))
            {
                return null;
            }
            var sourceRect = new Rectangle(Convert.ToInt32(region.X), Convert.ToInt32(region.Y),
                                           Convert.ToInt32(region.Width), Convert.ToInt32(region.Height));
            var destRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

            if (ScreenSource != null)
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    g.DrawImage(screen, destRect, sourceRect, GraphicsUnit.Pixel);
                }
                return bitmap;
            }
            return null;
        }
    }
}
