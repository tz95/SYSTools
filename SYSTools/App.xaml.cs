using iNKORE.UI.WPF.TrayIcons;
using System.Windows;
using SYSTools.Model;

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
            base.OnStartup(e);
            
            TaskbarIcon = (TrayIcon)FindResource("Taskbar");
            TaskbarIcon.ToolTipText = "SYSTools Ver" + (Application.ResourceAssembly.GetName().Version.ToString());
            
            // 设置默认语言
            string languageCode = AppSettings.Instance.Language ?? "zh-CN";
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(languageCode);
        }
    }
}
