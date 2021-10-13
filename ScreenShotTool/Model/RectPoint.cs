using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    public class RectPoint
    {
        public RectPoint(RectPointLocation location)
        {
            pointLocation = location;
        }
        private Rectangle point;
        public Rectangle Point
        {
            get { return point; }
        }
        /// <summary>
        /// 点的实际位置
        /// </summary>
        public RectPointLocation pointLocation { get; set; }
        public Brush pointFill { get; set; } = new SolidColorBrush(Color.FromRgb(26, 173, 25));
        public double pointThickness { get; set; } = 1;
        public double pointWidth { get; set; } = 6;
        public double pointHeight { get; set; } = 6;
        public double pointMargin { get; set; } = 3;
        /// <summary>
        /// 绘制点
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Rectangle Draw()
        {
            point = new Rectangle();
            point.Fill = pointFill;
            point.Width = pointWidth;
            point.Height = pointHeight;
            point.StrokeThickness = pointThickness;
            point.SnapsToDevicePixels = true;
            point.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            return point;
        }

        /// <summary>
        /// 清空矩形在画布上的位置信息
        /// </summary>
        private void ClearPosition()
        {
            point.SetValue(Canvas.LeftProperty, DependencyProperty.UnsetValue);
            point.SetValue(Canvas.RightProperty, DependencyProperty.UnsetValue);
            point.SetValue(Canvas.TopProperty, DependencyProperty.UnsetValue);
            point.SetValue(Canvas.BottomProperty, DependencyProperty.UnsetValue);
        }
        public void UpdatePosition(double width, double height)
        {
            ClearPosition();
            switch (pointLocation)
            {
                case RectPointLocation.LT:
                    point.Margin = new Thickness(-pointMargin, -pointMargin, 0, 0);
                    point.Cursor = Cursors.SizeNWSE;
                    Canvas.SetLeft(point, 0);
                    Canvas.SetTop(point, 0);
                    break;
                case RectPointLocation.RT:
                    point.Margin = new Thickness(0, -pointMargin, -pointMargin, 0);
                    point.Cursor = Cursors.SizeNESW;
                    Canvas.SetRight(point, 0);
                    Canvas.SetTop(point, 0);
                    break;
                case RectPointLocation.T:
                    point.Margin = new Thickness(0, -pointMargin, -pointMargin, 0);
                    point.Cursor = Cursors.SizeNS;
                    Canvas.SetTop(point, 0);
                    Canvas.SetLeft(point, (width / 2) - pointWidth / 2);
                    break;
                case RectPointLocation.B:
                    point.Margin = new Thickness(0, 0, 0, -pointMargin);
                    point.Cursor = Cursors.SizeNS;
                    Canvas.SetBottom(point, 0);
                    Canvas.SetLeft(point, (width / 2) - pointWidth / 2);
                    break;
                case RectPointLocation.L:
                    point.Margin = new Thickness(-pointMargin, 0, 0, 0);
                    point.Cursor = Cursors.SizeWE;
                    Canvas.SetLeft(point, 0);
                    Canvas.SetTop(point, (height / 2) - pointWidth / 2);
                    break;
                case RectPointLocation.R:
                    point.Margin = new Thickness(0, 0, -pointMargin, 0);
                    point.Cursor = Cursors.SizeWE;
                    Canvas.SetRight(point, 0);
                    Canvas.SetTop(point, (height / 2) - pointWidth / 2);
                    break;
                case RectPointLocation.LB:
                    point.Margin = new Thickness(-pointMargin, 0, 0, -pointMargin);
                    point.Cursor = Cursors.SizeNESW;
                    Canvas.SetLeft(point, 0);
                    Canvas.SetBottom(point, 0);
                    break;
                case RectPointLocation.RB:
                    point.Margin = new Thickness(0, 0, -pointMargin, -pointMargin);
                    point.Cursor = Cursors.SizeNWSE;
                    Canvas.SetRight(point, 0);
                    Canvas.SetBottom(point, 0);
                    break;
                default:
                    break;
            }
        }
    }

    public enum RectPointLocation
    {
        /// <summary>
        /// 左上角
        /// </summary>
        LT = 0,
        /// <summary>
        /// 上
        /// </summary>
        T = 1,
        /// <summary>
        /// 右上角
        /// </summary>
        RT = 2,
        /// <summary>
        /// 左
        /// </summary>
        L = 3,
        /// <summary>
        /// 右
        /// </summary>
        R = 4,
        /// <summary>
        /// 左下角
        /// </summary>
        LB = 5,
        /// <summary>
        /// 下
        /// </summary>
        B = 6,
        /// <summary>
        /// 右下角
        /// </summary>
        RB = 7
    }
}
