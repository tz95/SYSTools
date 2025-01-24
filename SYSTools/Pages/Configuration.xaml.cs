using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SYSTools.Model;

namespace SYSTools.Pages
{
    /// <summary>
    /// Configuration.xaml 的交互逻辑
    /// </summary>
    public partial class Configuration : Page
    {
        public Configuration()
        {
            InitializeComponent();
            LoadUserSettings();
            DataContext = this;
        }

        // 背景图片设定按钮
        private void SelectBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                AppSettings.Instance.BackgroundImagePath = openFileDialog.FileName;
                LoadBackgroundImage(openFileDialog.FileName);
            }
        }

        // 背景图片清除按钮(设定为内置透明图片)
        private void DeleteBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.BackgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
            LoadBackgroundImage("");
        }

        // 加载背景图片到预览窗口
        private void LoadBackgroundImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                BackgroundPreview.Source = null;
                return;
            }
            else
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.EndInit();
                    BackgroundPreview.Source = bitmap;
                }
                catch (Exception)
                {
                    BackgroundPreview.Source = null;
                }
            }
        }

        // 加载用户Config文件
        private void LoadUserSettings()
        {
            string savedImagePath = AppSettings.Instance.BackgroundImagePath;
            if (!string.IsNullOrWhiteSpace(savedImagePath) && File.Exists(savedImagePath))
            {
                LoadBackgroundImage(savedImagePath);
            }
        }

        private void BackgroundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            BackImageSettingsExpander.IsExpanded = BackgroundToggle.IsOn;
        }
    }
}
