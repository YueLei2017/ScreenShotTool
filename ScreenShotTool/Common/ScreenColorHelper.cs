using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ScreenShotTool
{
    public class ScreenColorHelper
    {
        //private struct POINTAPI
        //{
        //    public uint X;
        //    public uint Y;
        //}

        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);
        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);
        //[DllImport("user32")]
        //private static extern int GetCursorPos(out POINTAPI lpPoint);
        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);


        public static Color GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);
            //
            // Release the DC after getting the Color.
            ReleaseDC(0, lDC);

            //byte a = (byte)( ( intColor >> 0x18 ) & 0xffL );
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);
            Color color = Color.FromRgb(r, g, b);
            return color;
        }
    }
}
