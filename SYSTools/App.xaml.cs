using iNKORE.UI.WPF.TrayIcons;
using System.Windows;

namespace SYSTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static TrayIcon TaskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            TaskbarIcon = (TrayIcon)FindResource("Taskbar");
            TaskbarIcon.ToolTipText = "SYSTools Ver" + (Application.ResourceAssembly.GetName().Version.ToString());
        }
    }
}
