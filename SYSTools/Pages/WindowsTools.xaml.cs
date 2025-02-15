using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using System.Collections.Generic;
using SYSTools.WindowsToolsPages;
using Page = System.Windows.Controls.Page;


namespace SYSTools.Pages
{
    /// <summary>
    /// WindowsTools.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsTools : Page
    {
        private readonly Dictionary<Type, Page> _pages = new()
        {
            { typeof(WindowsUtilities), new WindowsUtilities() },
            { typeof(WindowsUpdateHistory), new WindowsUpdateHistory() }
        };

        public WindowsTools()
        {
            InitializeComponent();
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


        private void Window_Loading(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigateTo(typeof(WindowsUtilities), new DrillInNavigationTransitionInfo());
        }
    }
}
