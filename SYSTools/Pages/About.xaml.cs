using AutoUpdaterDotNET;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SYSTools.Properties;

namespace SYSTools.Pages
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        private static readonly HttpClient Client = new HttpClient();

        public About()
        {
            InitializeComponent();
            DataContext = this;
            
            var libraries = new List<Library>
            {
                new Library { Name = "AutoUpdater.NET (MIT License)", Uri = "https://github.com/ravibpatel/AutoUpdater.NET" },
                new Library { Name = "iNKORE.UI.WPF (LGPL-2.1 license)", Uri = "https://github.com/iNKORE-NET/UI.WPF" },
                new Library { Name = "iNKORE.UI.WPF.Modern (LGPL-2.1 license)", Uri = "https://github.com/iNKORE-Public/UI.WPF.Modern" },
                new Library { Name = "LibreHardwareMonitorLib", Uri = "https://github.com/LibreHardwareMonitor/LibreHardwareMonitor" },
                new Library { Name = "Log4Net (Apache-2.0 License)", Uri = "https://logging.apache.org/log4net/" },
                new Library { Name = "Microsoft.Windows.SDK.Contracts", Uri = "https://aka.ms/WinSDKProjectURL" },
                new Library { Name = "System.Runtime.WindowsRuntime", Uri = "https://github.com/dotnet/corefx" },
                new Library { Name = "System.Runtime.WindowsRuntime.UI.Xaml", Uri = "https://github.com/dotnet/corefx" },
                new Library { Name = "System.ValueTuple", Uri = "https://dot.net" },
                new Library { Name = "TextCopy", Uri = "https://github.com/CopyText/TextCopy" },
                new Library { Name = ".NET Runtime", Uri = "https://github.com/dotnet/runtime" },
                new Library { Name = "HidSharp", Uri = "http://www.zer7.com/software/hidsharp" },
                new Library { Name = "Microsoft.Bcl.AsyncInterfaces", Uri = "https://dot.net/" },
                new Library { Name = "Microsoft.Extensions.DependencyInjection.Abstractions", Uri = "https://dot.net/" },
                new Library { Name = "Microsoft.Web.WebView2", Uri = "https://aka.me/webview" },
                new Library { Name = "System.CodeDom", Uri = "https://dot.net/" },
                new Library { Name = "System.Management", Uri = "https://dot.net/" },
                new Library { Name = "System.Runtime.CompilerServices.Unsafe", Uri = "https://dot.net/" },
                new Library { Name = "System.Runtime.InteropServices.RuntimeInformation", Uri = "https://dot.net/" },
                new Library { Name = "System.Runtime.InteropServices.WindowsRuntime", Uri = "https://dot.net/" },
                new Library { Name = "System.Threading.Tasks.Extensions", Uri = "https://dot.net/" }
            };
            
            librariesItemsControl.ItemsSource = libraries;
        }

        private void QQ_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://jq.qq.com/?_wv=1027&k=qTaIu5Fa");
        }

        private void Web_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://systools.hksstudio.work");
        }


        private void AutoUpdateVersion() {
            AutoUpdater.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(800, 600);
            AutoUpdater.Start("https://systools.hksstudio.work/SYSTools_AutoUpdate.xml");
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            //软件本体检查更新
            HttpResponseMessage response = await Client.GetAsync("https://systools.hksstudio.work/SYSTools_Update_Version");
            response.EnsureSuccessStatusCode();
            string webCode = await response.Content.ReadAsStringAsync();

            Version currentVersion = System.Windows.Application.ResourceAssembly.GetName().Version;
            Version webVersion = new Version(webCode);

            if (currentVersion >= webVersion)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("暂未获取更新", "暂无更新🤐",MessageBoxButton.OK,SegoeFluentIcons.UpdateRestore);
            }
            else
            {
                AutoUpdateVersion();
            }
        }

        private async void Tool_Update_Click(object sender, RoutedEventArgs e)
        {
            //工具包检查更新
            string ToolsExecutablePath = Path.Combine(Directory.GetCurrentDirectory(), "SYSTools_Update(Tools).exe");

            FileVersionInfo ToolsFileVersionInfo = FileVersionInfo.GetVersionInfo(ToolsExecutablePath);

            HttpResponseMessage response = await Client.GetAsync("https://systools.hksstudio.work/Tools_Update/Tools_Update_Version");
            response.EnsureSuccessStatusCode();
            string webCode = await response.Content.ReadAsStringAsync();

            if (ToolsFileVersionInfo.FileVersion == webCode)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("暂未获取更新", "暂无更新🤐", MessageBoxButton.OK, SegoeFluentIcons.UpdateRestore);
            }
            else
            {
                Process.Start(ToolsExecutablePath);
            }
        }

        private async void Privacy_Click(object sender, RoutedEventArgs e)
        {
            // 从URL下载txt内容
            string url = "https://systools.hksstudio.work/Agree_Privacy/Privacy.txt";
            string txtContent = await GetTxtFromUrlAsync(url);

            // 创建并显示ContentDialog
            iNKORE.UI.WPF.Modern.Controls.ContentDialog dialog = new iNKORE.UI.WPF.Modern.Controls.ContentDialog
            {
                Title = "SYSTools 隐私协议",
                Content = new System.Windows.Controls.TextBox
                {
                    Text = txtContent,
                    AcceptsReturn = true,
                    AcceptsTab = true,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    IsReadOnly = true,
                    Padding = new Thickness(10, 0, 20, 0)
                },

                CloseButtonText = "关闭",
                PrimaryButtonText = "打开Url查看",
                DefaultButton = iNKORE.UI.WPF.Modern.Controls.ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();
            // 设定Url跳转地址
            if (result == iNKORE.UI.WPF.Modern.Controls.ContentDialogResult.Primary)
            {
                Process.Start(new ProcessStartInfo("https://systools.hksstudio.work/privacy.html") { UseShellExecute = true });
            }
        }

        private void Agreement_Click(object sender, RoutedEventArgs e)
        {

        }

        // 获取txt文件内容，GB2312编码
        private async Task<string> GetTxtFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes = await client.GetByteArrayAsync(url);
                return Encoding.GetEncoding("GB2312").GetString(bytes);
            }
        }

        private void OnCardClicked_Repository(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/Hikarisame-Technology/SYSTools") { UseShellExecute = true });
        }

        private void OnCardClicked_Issue(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/Hikarisame-Technology/SYSTools/issues") { UseShellExecute = true });
        }

    }

    public class Library
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}
