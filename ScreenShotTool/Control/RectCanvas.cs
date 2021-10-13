using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    public class RectCanvas : Canvas
    {
        #region 画板元素,8个实心矩形,4条边线
        //点集合
        private readonly RectPointCollection rectPoints = new RectPointCollection();

        //上方直线
        private readonly Line TopLine = new Line();
        private readonly Line LeftLine = new Line();
        private readonly Line RightLine = new Line();
        private readonly Line BottomLine = new Line();

        public Brush CanvasStroke { get; set; } = new SolidColorBrush(Color.FromRgb(26, 173, 25));
        private const double PointThickness = 1;
        private const double PointWidth = 6;
        private const double PointMargin = 3;
        private const double LineThickness = 1;

        private MaskCanvas ParentCanvas { get; set; }
        #endregion
        //鼠标移动事件,供外部订阅,用于改变截图框大小
        public delegate void PointMoveEventHandler(Vector vector, RectPointLocation location, RectPointCollection rectPoints);
        public event PointMoveEventHandler PointMove;

        public RectCanvas()
        {
            Background = Brushes.Transparent;
            Cursor = Cursors.SizeAll;
            Loaded += RectCanvas_Loaded;
            Unloaded += RectCanvas_Unloaded;
        }

        /// <summary>
        /// 构造点与线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RectCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            TopLine.Stroke = LeftLine.Stroke = RightLine.Stroke = BottomLine.Stroke = CanvasStroke;
            TopLine.StrokeThickness = LeftLine.StrokeThickness = RightLine.StrokeThickness 
                                    = BottomLine.StrokeThickness = LineThickness;
            TopLine.SnapsToDevicePixels = true;
            TopLine.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            LeftLine.SnapsToDevicePixels = true;
            LeftLine.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            RightLine.SnapsToDevicePixels = true;
            RightLine.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            BottomLine.SnapsToDevicePixels = true;
            BottomLine.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            Children.Add(TopLine);
            Children.Add(LeftLine);
            Children.Add(RightLine);
            Children.Add(BottomLine);

            foreach (RectPoint rectPoint in rectPoints.SetPointFill(CanvasStroke)
                                                      .SetPointThickness(PointThickness)
                                                      .SetPointWidthAndHeight(PointWidth)
                                                      .SetPointMargin(PointMargin))
            {
                Rectangle rectangle = rectPoint.Draw();
                rectangle.MouseLeftButtonDown += RectPointMouseLeftButtonDown;
                rectangle.MouseLeftButtonUp += RectPointMouseLeftButtonUp;
                rectangle.MouseMove += RectPointMouseMove;
                Children.Add(rectangle);
            }

            ParentCanvas = LogicalTreeHelper.GetParent(this) as MaskCanvas;
        }

        private void RectCanvas_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (RectPoint rectPoint in rectPoints.SetPointFill(CanvasStroke)
                                                      .SetPointThickness(PointThickness)
                                                      .SetPointWidthAndHeight(PointWidth)
                                                      .SetPointMargin(PointMargin))
            {
                rectPoint.Point.MouseLeftButtonDown -= RectPointMouseLeftButtonDown;
                rectPoint.Point.MouseLeftButtonUp -= RectPointMouseLeftButtonUp;
                rectPoint.Point.MouseMove -= RectPointMouseMove;
            }
            Children.Clear();
        }

        private Point? moveStartPoint;
        private Point? moveEndPoint;
        /// <summary>
        /// 鼠标左键点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RectPointMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                rectangle.CaptureMouse();
                moveStartPoint = e.GetPosition(ParentCanvas);
            }
        }
        /// <summary>
        /// 鼠标左键释放事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RectPointMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                rectangle.ReleaseMouseCapture();
                moveStartPoint = null;
                moveEndPoint = null;
            }
        }
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RectPointMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Rectangle rectangle = sender as Rectangle;
                if (rectangle != null && moveStartPoint.HasValue)
                {
                    moveEndPoint = e.GetPosition(ParentCanvas);
                    Vector vector = moveEndPoint.Value - moveStartPoint.Value;
                    PointMove?.Invoke(vector, rectPoints.FindLocationbyRect(rectangle).Value, rectPoints);
                    moveStartPoint = new Point?(moveEndPoint.Value);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// 更新点与线的位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdatePointAndLine()
        {
            double actualHeight = Height;
            double actualWidth = Width;
            //根据Canvas大小更新点和直线
            rectPoints.UpdatePointPosition(actualWidth, actualHeight);

            TopLine.X1 = TopLine.Y1 = 0;
            TopLine.X2 = actualWidth;
            TopLine.Y2 = 0;

            LeftLine.X1 = LeftLine.X2 = 0;
            LeftLine.Y1 = 0;
            LeftLine.Y2 = actualHeight;

            RightLine.X1 = RightLine.X2 = actualWidth;
            RightLine.Y1 = 0;
            RightLine.Y2 = actualHeight;

            BottomLine.Y1 = BottomLine.Y2 = actualHeight;
            BottomLine.X1 = 0;
            BottomLine.X2 = actualWidth;
        }
    }
}
