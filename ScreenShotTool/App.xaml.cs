using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenShotTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += (sender, ue) =>
            {
                MessageBox.Show(ue.Exception.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ue.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, due) =>
            {
                MessageBox.Show((due.ExceptionObject as Exception).Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            base.OnStartup(e);
        }
    }
}
