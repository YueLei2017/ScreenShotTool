using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    public class MaskCanvas : Canvas
    {
        //4个蒙版 + 1个截图区
        private readonly Rectangle maskLeftRect = new Rectangle();
        private readonly Rectangle maskRightRect = new Rectangle();
        private readonly Rectangle maskTopRect = new Rectangle();
        private readonly Rectangle maskBottomRect = new Rectangle();
        private readonly RectCanvas selectionRect = new RectCanvas();
        //左上角截图范围显示
        private readonly TextBlock selectionRange = new TextBlock();
        //框选的矩形
        private Rect selectionRegion = Rect.Empty;
        //截图开始和结束点
        private Point? selectionStartPoint;
        private Point? selectionEndPoint;
        //工具栏
        private readonly ToolBarControl toolBar = new ToolBarControl();
        //放大镜
        private readonly ZoomControl zoom = new ZoomControl();
        //截图大小和工具栏的间隙
        private const double ToolBarMagrin = 5;
        //放大镜中的位图对象
        private System.Drawing.Bitmap bitmap;
        public MaskCanvas()
        {
            Loaded += MaskCanvas_Loaded;
            Unloaded += MaskCanvas_Unloaded;

            maskLeftRect.Fill = maskRightRect.Fill = maskTopRect.Fill = maskBottomRect.Fill = Brushes.Black;
            maskLeftRect.Opacity = maskRightRect.Opacity = maskTopRect.Opacity = maskBottomRect.Opacity = 0.5;
        }


        private MaskingWindowView ParentView { get; set; }

        /// <summary>
        /// 初始化截图画板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaskCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            SetLeft(maskLeftRect, 0);
            SetTop(maskLeftRect, 0);
            SetRight(maskRightRect, 0);
            SetTop(maskRightRect, 0);
            SetTop(maskTopRect, 0);
            SetBottom(maskBottomRect, 0);
            maskLeftRect.Height = ActualHeight;
            maskLeftRect.Width = ActualWidth;

            Children.Add(maskLeftRect);
            Children.Add(maskRightRect);
            Children.Add(maskTopRect);
            Children.Add(maskBottomRect);

            selectionRect.Visibility = Visibility.Collapsed;
            Children.Add(selectionRect);
            selectionRect.PointMove += SelectionRect_PointMove;

            selectionRange.Visibility = Visibility.Collapsed;
            selectionRange.Height = 21;
            selectionRange.Width = 80;
            selectionRange.Foreground = Brushes.White;
            Children.Add(selectionRange);

            toolBar.Visibility = Visibility.Collapsed;
            toolBar.OnExit += ExitWindow;
            toolBar.OnComplete += SaveCaptureAction;
            toolBar.OnSave += SaveCaptureAction;
            Children.Add(toolBar);

            Children.Add(zoom);
            //按照屏幕刷新率去更新遮罩和截图框的位置
            CompositionTarget.Rendering += UpdateRectangle;
            ParentView = LogicalTreeHelper.GetParent(this) as MaskingWindowView;

            locakBitmap = new LockBitmap((ParentView.DataContext as MaskingWindowViewModel).screen);
        }

        private void MaskCanvas_Unloaded(object sender, RoutedEventArgs e)
        {
            //退出时将订阅全部取消
            Children.Clear();
            selectionRect.PointMove -= SelectionRect_PointMove;

            toolBar.OnExit -= ExitWindow;
            toolBar.OnComplete -= SaveCaptureAction;
            toolBar.OnSave -= SaveCaptureAction;

            CompositionTarget.Rendering -= UpdateRectangle;
            bitmap?.Dispose();
            bitmap = null;
        }

        private void SelectionRect_PointMove(Vector vector, RectPointLocation location, RectPointCollection rectPoints)
        {
            if (!selectionRegion.IsEmpty)
            {
                Rect tempRegion = selectionRegion;
                double resultX = tempRegion.Left;
                //上下两点不考虑Y轴偏移
                if (!(location == RectPointLocation.T || location == RectPointLocation.B))
                {
                    if(location == RectPointLocation.L || location == RectPointLocation.LT
                        || location == RectPointLocation.LB)
                    {
                        if(resultX + vector.X > tempRegion.Right)
                        {
                            resultX = tempRegion.Right;
                            //发生Y轴偏
                            rectPoints.OverTurnPoint(AxleType.Y);
                        }
                        else
                        {
                            resultX += vector.X;
                        }
                    }
                    else if (tempRegion.Right + vector.X < tempRegion.Left)
                    {
                        resultX = tempRegion.Right + vector.X;
                        //发生Y轴偏
                        rectPoints.OverTurnPoint(AxleType.Y);
                    }
                }
                double resultY = tempRegion.Top;
                //左右两点不考虑X轴偏移
                if (!(location == RectPointLocation.L || location == RectPointLocation.R))
                {
                    if (location == RectPointLocation.T || location == RectPointLocation.LT
                        || location == RectPointLocation.RT)
                    {
                        if (resultY + vector.Y > tempRegion.Bottom)
                        {
                            resultY = tempRegion.Bottom;
                            //发生X轴偏
                            rectPoints.OverTurnPoint(AxleType.X);
                        }
                        else
                        {
                            resultY += vector.Y;
                        }
                    }
                    else if (tempRegion.Bottom + vector.Y < tempRegion.Top) 
                    {
                        resultY = tempRegion.Bottom + vector.Y;
                        //发生X轴偏
                        rectPoints.OverTurnPoint(AxleType.X);
                    }
                }
                double resultWidth = tempRegion.Width;
                //上下两点宽度不变化
                if (!(location == RectPointLocation.T || location == RectPointLocation.B))
                {
                    if (location == RectPointLocation.L || location == RectPointLocation.LT
                        || location == RectPointLocation.LB)
                    {
                        //左侧点移动,原始宽度减去偏移量
                        resultWidth -= vector.X;
                    }
                    else
                    {
                        //右侧点移动,加上偏移量
                        resultWidth += vector.X;
                    }
                    resultWidth = Math.Abs(resultWidth);
                }
                double resultHeight = tempRegion.Height;
                //左右两点高度不变化
                if (!(location == RectPointLocation.L || location == RectPointLocation.R))
                {
                    if (location == RectPointLocation.T || location == RectPointLocation.LT
                        || location == RectPointLocation.RT)
                    {
                        //上侧点移动,原始高度减去偏移量
                        resultHeight -= vector.Y;
                    }
                    else
                    {
                        //下侧点移动,原始高度加上偏移量
                        resultHeight += vector.Y;
                    }
                    resultHeight = Math.Abs(resultHeight);
                }
                tempRegion.X = Math.Max(resultX, 1);
                tempRegion.Y = Math.Max(resultY, 1);
                tempRegion.Width = resultWidth;
                tempRegion.Height = resultHeight;
                selectionRegion = tempRegion;

                //更改截图框大小时,显示放大镜
                zoom.Visibility = Visibility.Visible;
            }
        }

        #region 事件
        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(e.Source.Equals(maskLeftRect) || e.Source.Equals(maskRightRect)
                || e.Source.Equals(maskTopRect) || e.Source.Equals(maskBottomRect))
            {
                //如果已有截图则不处理该事件
                if (selectionRegion != Rect.Empty)
                {
                    return;
                }
                selectionStartPoint = new Point?(e.GetPosition(this));
                selectionRect.Visibility = Visibility.Visible;
                selectionRange.Visibility = Visibility.Visible;
                CaptureMouse();
            }
            else if(e.Source.Equals(selectionRect))
            {
                //双击截图区保存图片
                if (e.ClickCount >= 2)
                {
                    FinishAction();
                }
                else
                {
                    dragging = true;
                    draggingStart = new Point?(e.GetPosition(this));
                    selectionRect.CaptureMouse();
                    //拖动时工具栏隐藏
                    toolBar.Visibility = Visibility.Collapsed;
                }
            }
            else if(e.Source is Rectangle)
            {
                //按住小点时工具栏隐藏
                toolBar.Visibility = Visibility.Collapsed;
                //按住小点,显示放大镜
                zoom.Visibility = Visibility.Visible;
            }
            base.OnMouseLeftButtonDown(e);
        }

        private bool dragging = false;
        private Point? draggingStart = null;
        private Point? draggingEnd = null;
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //更新选中区
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectionStartPoint.HasValue)
                {
                    selectionEndPoint = e.GetPosition(this);
                    Point startPoint = selectionStartPoint.Value;
                    Point endPoint = selectionEndPoint.Value;
                    double x = Math.Max(Math.Min(startPoint.X, endPoint.X), 1);
                    double y = Math.Max(Math.Min(startPoint.Y, endPoint.Y), 1);
                    double width = Math.Min(Math.Abs(endPoint.X - startPoint.X), ActualWidth - 1 - x);
                    double height = Math.Min(Math.Abs(endPoint.Y - startPoint.Y), ActualHeight - 1 - y);
                    Rect tempRect = new Rect(x, y, width, height);

                    selectionRegion = tempRect;

                    //按下鼠标构造截图框时,显示放大镜
                    zoom.Visibility = Visibility.Visible;
                }
                //拖动选择区
                else if(dragging)
                {
                    draggingEnd = new Point?(e.GetPosition(this));

                    //计算两个之间的差值,然后修改selectionRegion
                    Point start = draggingStart.Value;
                    Point end = draggingEnd.Value;
                    Vector vector = end - start;
                    Rect tempRegion = selectionRegion;
                    double resultX = tempRegion.X + vector.X;
                    double resultY = tempRegion.Y + vector.Y;
                    //不可小于1,不可超出屏幕边界
                    resultX = Math.Min(Math.Max(resultX, 1), ActualWidth - 1 - tempRegion.Width);
                    resultY = Math.Min(Math.Max(resultY, 1), ActualHeight - 1 - tempRegion.Height);

                    tempRegion.X = resultX;
                    tempRegion.Y = resultY;
                    selectionRegion = tempRegion;

                    draggingStart = new Point?(e.GetPosition(this));
                }
            }
            else if(selectionRegion.IsEmpty)
            {
                //未按下鼠标移动且没有截图框时,显示放大镜
                zoom.Visibility = Visibility.Visible;
            }
            base.OnMouseMove(e);
        }
        /// <summary>
        /// 鼠标左键抬起事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            selectionStartPoint = null;
            selectionEndPoint = null;
            dragging = false;
            draggingStart = null;
            draggingEnd = null;
            ReleaseMouseCapture();
            selectionRect.ReleaseMouseCapture();
            //鼠标抬起,显示工具栏并定位到矩形框的右下角
            UpdateToolBarPosition();
            //抬起鼠标,不展示放大镜
            zoom.Visibility = Visibility.Collapsed;
            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>
        /// 鼠标右键抬起事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            //点击右键去除已截图的区域
            if (selectionStartPoint.HasValue || selectionRegion.IsEmpty)
            {
                ParentView.Close();
            }
            else
            {
                selectionRegion = Rect.Empty;
                selectionRect.Width = selectionRect.Height = 0;
                selectionRect.Visibility = Visibility.Collapsed;
                selectionRange.Visibility = Visibility.Collapsed;
                toolBar.Visibility = Visibility.Collapsed;
                selectionStartPoint = null;
                selectionEndPoint = null;
                ReleaseMouseCapture();
                selectionRect.ReleaseMouseCapture();
                zoom.Visibility = Visibility.Visible;
            }

            base.OnMouseRightButtonUp(e);
        }
        /// <summary>
        /// 界面绘制事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRectangle(object sender, EventArgs e)
        {
            //更新截图区和遮罩区
            UpdateSelectionRectLayout();
            UpdateMaskingRectLayout();
            //更新矩形范围
            UpdateSelectionRange();
            if (zoom.Visibility == Visibility.Visible)
            {
                UpdateZoomPosition();
            }
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 更新截图区
        /// </summary>
        private void UpdateSelectionRectLayout()
        {
            if (!selectionRegion.IsEmpty)
            {
                Rect tempRegion = selectionRegion;

                SetLeft(selectionRect, tempRegion.Left);
                SetTop(selectionRect, tempRegion.Top);
                selectionRect.Width = tempRegion.Width;
                selectionRect.Height = tempRegion.Height;
                selectionRect.UpdatePointAndLine();
            }
        }

        /// <summary>
        /// 更新遮罩区
        /// </summary>
        private void UpdateMaskingRectLayout()
        {
            double actualHeight = ActualHeight;
            double actualWidth = ActualWidth;

            if (selectionRegion.IsEmpty)
            {
                SetLeft(maskLeftRect, 0);
                SetTop(maskLeftRect, 0);
                maskLeftRect.Height = actualHeight;
                maskLeftRect.Width = actualWidth;

                maskRightRect.Width = maskRightRect.Height = maskTopRect.Width = maskTopRect.Height = maskBottomRect.Width = maskBottomRect.Height = 0;
            }
            else
            {
                Rect tempRegion = selectionRegion;

                //设置左遮罩
                SetLeft(maskLeftRect, 0);
                SetTop(maskLeftRect, 0);
                maskLeftRect.Width = tempRegion.Left;
                maskLeftRect.Height = actualHeight;

                //设置右遮罩
                SetRight(maskRightRect, 0);
                SetTop(maskRightRect, 0);
                maskRightRect.Width = Math.Max(actualWidth - tempRegion.Right, 0);
                maskRightRect.Height = actualHeight;

                //设置上遮罩
                SetLeft(maskTopRect, maskLeftRect.Width);
                SetTop(maskTopRect, 0);
                maskTopRect.Width = Math.Max(actualWidth - maskLeftRect.Width - maskRightRect.Width, 0);
                maskTopRect.Height = tempRegion.Top;

                //设置下遮罩
                SetLeft(maskBottomRect, maskLeftRect.Width);
                SetBottom(maskBottomRect, 0);
                maskBottomRect.Width = maskTopRect.Width;
                maskBottomRect.Height = Math.Max(actualHeight - tempRegion.Bottom, 0);
            }
        }
        /// <summary>
        /// 更新截图区大小
        /// </summary>
        private void UpdateSelectionRange()
        {
            //Label永远在矩形外部的左上角,除非可能超过屏幕,才变为矩形内部左上角.
            //格式为 [长 x 高] 最小一个像素
            if (!selectionRegion.IsEmpty)
            {
                Rect tempRegion = selectionRegion;

                SetLeft(selectionRange, tempRegion.Left + ToolBarMagrin);
                if(tempRegion.Top < selectionRange.Height)
                {
                    SetTop(selectionRange, tempRegion.Top);
                }
                else
                {
                    SetTop(selectionRange, tempRegion.Top - selectionRange.Height);
                }
                string rangestr = Math.Max((int)tempRegion.Width, 1) + " x " + Math.Max((int)tempRegion.Height, 1);
                selectionRange.Text = rangestr;
            }
        }
        /// <summary>
        /// 更新工具栏位置
        /// </summary>
        private void UpdateToolBarPosition()
        {
            //工具栏始终在矩形底部靠右,除非左侧可能冲出屏幕,则改为靠左
            //如果底部可能冲出屏幕,则在顶部靠右,如果上下都冲出屏幕,则在右下角矩形内部靠右
            if (!selectionRegion.IsEmpty)
            {
                Rect tempRegion = selectionRegion;
                double margin = 0;
                //将顶出下屏幕
                if (tempRegion.Bottom + ToolBarControl.ControlHeight >= ActualHeight - ToolBarMagrin)
                {
                    //将顶出上屏幕
                    if (tempRegion.Top < (ToolBarControl.ControlHeight + ToolBarMagrin))
                    {
                        SetTop(toolBar, tempRegion.Bottom - ToolBarControl.ControlHeight - ToolBarMagrin);
                        //位置在矩形框内部,需要左右留出5的空隙
                        margin = 5;
                    }
                    else
                    {
                        SetTop(toolBar, tempRegion.Top - ToolBarControl.ControlHeight - ToolBarMagrin);
                    }
                }
                else
                {
                    SetTop(toolBar, tempRegion.Bottom + ToolBarMagrin);
                }


                //将顶出左侧,则靠左
                if (tempRegion.Right - ToolBarControl.ControlWidth <= 0)
                {
                    SetLeft(toolBar, tempRegion.Left + margin);
                }
                //否则靠右
                else
                {
                    SetLeft(toolBar, tempRegion.Right - ToolBarControl.ControlWidth - margin);
                }
                toolBar.Visibility = Visibility.Visible;
            }
        }

        private LockBitmap locakBitmap;
        /// <summary>
        /// 更新放大镜位置
        /// </summary>
        private void UpdateZoomPosition()
        {
            Point mousePosition = Mouse.GetPosition(this);
            //放大镜始终在鼠标右下角20个像素处，整个Zoom控件宽115，高155。如果右侧碰撞，则移动至鼠标左侧，下侧碰撞则移动至鼠标上侧。距离中心都是20个像素。
            if (mousePosition.X + 20 + ZoomControl.ControlWidth + ToolBarMagrin >= ActualWidth)
            {
                SetLeft(zoom, mousePosition.X - 20 - ZoomControl.ControlWidth);
            }
            else
            {
                SetLeft(zoom, mousePosition.X + 20);
            }
            if (mousePosition.Y + 20 + ZoomControl.ControlHeight + 5 >= ActualHeight)
            {
                SetTop(zoom, mousePosition.Y - 20 - ZoomControl.ControlHeight);
            }
            else
            {
                SetTop(zoom, mousePosition.Y + 20);
            }
            Rect mouseRect = new Rect(mousePosition.X - 20, mousePosition.Y - 20, 40, 40);

            using (bitmap = new System.Drawing.Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                zoom.ZoomImage.Source = (ParentView.DataContext as MaskingWindowViewModel)
                                        .CopyBitmapFromScreenSnapshot(mouseRect, bitmap)
                                        .ToBitmapSource();
            }

            locakBitmap.LockBits();
            Color underMouseColor = locakBitmap.GetPixel((int)mousePosition.X, (int)mousePosition.Y);
            zoom.RGB.Text = "RGB：(" + underMouseColor.R + "," + underMouseColor.G + "," + underMouseColor.B + ")";
            locakBitmap.UnlockBits();

            zoom.POS.Text = "POS：(" + (int)mousePosition.X + "," + (int)mousePosition.Y + ")";
            zoom.Visibility = Visibility.Visible;
        }


        private void ExitWindow()
        {
            ParentView.Close();
        }

        /// <summary>
        /// 保存图片到桌面并退出截图
        /// </summary>
        public void FinishAction()
        {
            SaveCaptureAction();
        }

        private void SaveCaptureAction()
        {
            System.Drawing.Bitmap currentImage = GetSnapBitmap();
            if (currentImage == null)
            {
                return;
            }
            System.Windows.Forms.SaveFileDialog saveDlg = new System.Windows.Forms.SaveFileDialog();
            string mydocPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            saveDlg.InitialDirectory = mydocPath/* + "\\"*/;
            saveDlg.Filter = "PNG(*.png)|*.png|BMP(*.bmp)|*.bmp|JPEG(*.jpg)|*.jpg";
            saveDlg.FilterIndex = 1;
            saveDlg.FileName = "截图_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (saveDlg.FilterIndex)
                {
                    case 1:
                        currentImage.Save(saveDlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case 2:
                        currentImage.Clone(new System.Drawing.Rectangle(0, 0, currentImage.Width, currentImage.Height),
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb).Save(saveDlg.FileName,
                            System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 3:
                        currentImage.Save(saveDlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
                currentImage.Dispose();
                ParentView.Close();
            }
        }

        public System.Drawing.Bitmap GetSnapBitmap()
        {
            System.Drawing.Bitmap saveBitmap = null;
            if (ParentView != null && ParentView.DataContext != null)
            {
                Rect clipRegion = GetIndicatorRegion();
                saveBitmap = (ParentView.DataContext as MaskingWindowViewModel).CopyBitmapFromScreenSnapshot(clipRegion);
            }
            return saveBitmap;

        }

        private Rect GetIndicatorRegion()
        {
            return new Rect(GetLeft(selectionRect), GetTop(selectionRect), selectionRect.ActualWidth, selectionRect.ActualHeight);
        }
        #endregion
    }
}
