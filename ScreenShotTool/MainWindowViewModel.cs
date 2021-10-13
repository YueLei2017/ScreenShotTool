using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScreenShotTool
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {

        }
        public Window View { get; set; }

        private bool isHiddenWindow = false;
        public bool IsHiddenWindow
        {
            get { return isHiddenWindow; }
            set
            {
                SetProperty(ref isHiddenWindow, value);
            }
        }

        public async void ScreenShotClick(object sender, RoutedEventArgs e)
        {
            //如果勾选了隐藏窗口,则将现有的窗口隐藏后再截图
            if (isHiddenWindow)
            {
                View.Hide();
                await Task.Delay(200);
            }

            new MaskingWindowView().ShowDialog();
            //图像处理会导致内存上升很快,即时使用了Dispose也不会第一时间释放内存
            //在退出截图后进行一次GC垃圾回收
            GC.Collect();

            if (isHiddenWindow)
            {
                View.Show();
            }
        }

    }
}
