using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    /// <summary>
    /// MaskingWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class MaskingWindowView : Window
    {
        public MaskingWindowView()
        {
            InitializeComponent();
            DataContext = new MaskingWindowViewModel()
            {
                View = this
            };

        }
    }
}
