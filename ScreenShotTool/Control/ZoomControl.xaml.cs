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
    /// ZoomControl.xaml 的交互逻辑
    /// </summary>
    public partial class ZoomControl : UserControl
    {
        public const double ControlHeight = 155;
        public const double ControlWidth = 115;
        public ZoomControl()
        {
            InitializeComponent();
            Width = ControlWidth;
            Height = ControlHeight;
        }
    }
}
