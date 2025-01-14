﻿using iNKORE.UI.WPF.Modern.Controls;
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
            { typeof(Toolkit), new Toolkit() }
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            AppSettings.Instance.PropertyChanged += OnSettingsPropertyChanged;
        }

        // 监听全局设置修改
        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.BackgroundImagePath))
            {
                LoadBackgroundImage(AppSettings.Instance.BackgroundImagePath);
            }
            else if (e.PropertyName == nameof(AppSettings.BackgroundImageBlurRadius))
            {
                LoadBackgroundImageBlurRadius(AppSettings.Instance.BackgroundImageBlurRadius);
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
            // 加载设置并应用
            LoadBackgroundImage(AppSettings.Instance.BackgroundImagePath);
            LoadBackgroundImageBlurRadius(AppSettings.Instance.BackgroundImageBlurRadius);
        }

        private void LoadBackgroundImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                BackImage.Source = bitmap;
            }
            catch (Exception)
            {
                // 如果加载失败，使用默认图片
                LoadBackgroundImage("pack://application:,,,/Resources/NoBackImage.png");
            }
        }

        private void LoadBackgroundImageBlurRadius(double radiusInt)
        {
            var blurEffect = new BlurEffect
            {
                Radius = radiusInt
            };
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
