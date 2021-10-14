using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    /// <summary>
    /// ToolBarControl.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBarControl : UserControl
    {
        public const double ControlHeight = 37;
        public const double ControlWidth = 250;

        public delegate void SaveEventHandler();
        /// <summary>
        /// 保存事件
        /// </summary>
        public event SaveEventHandler OnSave;

        public delegate void ExitEventHandler();
        /// <summary>
        /// 退出事件
        /// </summary>
        public event ExitEventHandler OnExit;

        public delegate void CompleteEventHandler();
        /// <summary>
        /// 完成事件
        /// </summary>
        public event CompleteEventHandler OnComplete;
        public ToolBarControl()
        {
            InitializeComponent();
            Height = ControlHeight;
            Width = ControlWidth;
        }
        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            OnSave?.Invoke();
        }
        private void ExitClick(object sender, MouseButtonEventArgs e)
        {
            OnExit?.Invoke();
        }

        private void CompleteClick(object sender, MouseButtonEventArgs e)
        {
            OnComplete?.Invoke();
        }
    }
}
