using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using SYSTools.Model;
using SYSTools.Pages;
using Page = System.Windows.Controls.Page;

namespace SYSTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        private readonly Dictionary<Type, Page> _pages = new()
        {
            { typeof(Home), new Home() },
            { typeof(Test), new Test() },
            { typeof(WindowsTools), new WindowsTools() },
            { typeof(WSATools), new WSATools() },
            { typeof(Configuration), new Configuration() },
            { typeof(About), new About() },
            { typeof(Other), new Other() }
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            GlobalSettings.Instance.PropertyChanged += OnSettingsPropertyChanged;
        }

        // 监听全局设置修改
        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GlobalSettings.BackgroundImagePath))
            {
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.BackgroundImagePath))
                {
                    BackImage.Source = new BitmapImage(
                        new Uri(
                            GlobalSettings.Instance.BackgroundImagePath,
                            UriKind.RelativeOrAbsolute
                        )
                    );
                }
            }
            else if (e.PropertyName == nameof(GlobalSettings.BackgroundImageBlurRadius))
            {
                LoadBackgroundImageBlurRadius(GlobalSettings.Instance.BackgroundImageBlurRadius);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 启动时删除Info.xml
            File.Delete("Info.xml");
            // 程序启动数量限制
            string appName = Process.GetCurrentProcess().ProcessName;
            int processTotal = Process.GetProcessesByName(appName).Length;
            if (processTotal > 1)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    "有一个同名进程正在运行！",
                    "程序冲突!",
                    MessageBoxButton.OK
                );
                Close();
            }
            TitleBarTextBlock.Text = "SYSTools Ver" + (Application.ResourceAssembly.GetName().Version.ToString());
            // 设置默认启动Page页
            NavigateTo(typeof(Home), new DrillInNavigationTransitionInfo());
        }

        private void NavigationTriggered(
            NavigationView sender,
            NavigationViewItemInvokedEventArgs args
        )
        {
            if (args.InvokedItemContainer?.Tag is string tag)
            {
                Type targetType = Type.GetType(tag);
                if (targetType != null)
                {
                    // 统一切换动画
                    NavigateTo(targetType, new DrillInNavigationTransitionInfo());
                }
            }
        }

        private void NavigateTo(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // 导航到目标页
            var preNavPageType = CurrentPage.Content?.GetType();
            if (navPageType == preNavPageType) return;
            transitionInfo ??= new DrillInNavigationTransitionInfo();
            if (_pages.TryGetValue(navPageType, out var page))
            {
                CurrentPage.Navigate(page, transitionInfo);
            }
            else
            {
                // 处理未注册页面的逻辑
                throw new InvalidOperationException($"Page type {navPageType.Name} is not registered.");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile)
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Save();
                LoadUserSettings();
            // 加载用户配置
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            // 启动程序时加载 user.config 文件
            string savedImagePath = Properties.Settings.Default.BackgroundImagePath;
            double savedBlurRadius = Properties.Settings.Default.BackgroundImageBlurRadius;
            if (!string.IsNullOrWhiteSpace(savedImagePath) && File.Exists(savedImagePath))
            {
                LoadBackgroundImage(savedImagePath);
                LoadBackgroundImageBlurRadius(savedBlurRadius);
            }
            else
            {
                // 读取不到Config的背景图片路径或路径为空则设定为全透明图片路径
                LoadBackgroundImage("pack://application:,,,/Resources/NoBackImage.png");
                LoadBackgroundImageBlurRadius(savedBlurRadius);
            }
        }

        private void LoadBackgroundImage(string imagePath)
        {
            // 加载背景图片
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();
            BackImage.Source = bitmap;
        }

        private void LoadBackgroundImageBlurRadius(double RadiusInt)
        {
            // 加载背景模糊
            var blurEffect = new BlurEffect
            {
                // 获取模糊度
                Radius = RadiusInt 
            };
            // 为背景图片应用模糊效果
            BackImage.Effect = blurEffect; 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 结束线程
            Application.Current.Shutdown();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // 修改最小化窗口为隐藏
            if (WindowState == WindowState.Minimized)
            {
                Application.Current.MainWindow.Hide();
            }
        }
    }
}
