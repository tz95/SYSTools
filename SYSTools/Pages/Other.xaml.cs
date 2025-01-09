using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Media.Animation;
using System;
using SYSTools.ToolPages;
using Page = System.Windows.Controls.Page;


namespace SYSTools.Pages
{
    /// <summary>
    /// Other.xaml 的交互逻辑
    /// </summary>
    public partial class Other : Page
    {
        private readonly Page DetectionTools_Page = new DetectionTools();
        private readonly Page TestTools_Page = new TestTools();
        private readonly Page DiskTools_Page = new DiskTools();
        private readonly Page PeripheralsTools_Page = new PeripheralsTools();
        private readonly Page RepairingTools_Page = new RepairingTools();


        public Other()
        {
            InitializeComponent();
        }

        private void NavigationTriggered(
            NavigationView sender,
            NavigationViewItemInvokedEventArgs args
            )
        {
            if (args.InvokedItemContainer != null)
                NavigateTo(
                    Type.GetType(args.InvokedItemContainer.Tag.ToString()),
                    args.RecommendedNavigationTransitionInfo
                );
        }

        private void NavigateTo(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // 导航到目标页
            var preNavPageType = CurrentPage.Content.GetType();
            if (navPageType == preNavPageType)
                return;
            switch (navPageType)
            {
                case not null when navPageType == typeof(DetectionTools):
                    CurrentPage.Navigate(DetectionTools_Page);
                    break;
                case not null when navPageType == typeof(TestTools):
                    CurrentPage.Navigate(TestTools_Page);
                    break;
                case not null when navPageType == typeof(DiskTools):
                    CurrentPage.Navigate(DiskTools_Page);
                    break;
                case not null when navPageType == typeof(PeripheralsTools):
                    CurrentPage.Navigate(PeripheralsTools_Page);
                    break;
                case not null when navPageType == typeof(RepairingTools):
                    CurrentPage.Navigate(RepairingTools_Page);
                    break;
            }
        }

        private void Window_Loading(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentPage.Navigate(DetectionTools_Page, new DrillInNavigationTransitionInfo());
        }
    }
}
