using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace SYSTools.Model
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());
        public static AppSettings Instance => _instance.Value;
        private readonly string _settingsFilePath;

        private string _backgroundImagePath;
        private double _backgroundImageBlurRadius;

        public event PropertyChangedEventHandler PropertyChanged;

        private AppSettings()
        {
            // 设置配置文件路径
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsFilePath = Path.Combine(appDataPath, "HikarisameTechnologyStudio", "SYSTools", "settings.config");
            
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
            
            LoadSettings();
        }

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set
            {
                if (_backgroundImagePath != value)
                {
                    _backgroundImagePath = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public double BackgroundImageBlurRadius
        {
            get => _backgroundImageBlurRadius;
            set
            {
                if (_backgroundImageBlurRadius != value)
                {
                    _backgroundImageBlurRadius = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new XDocument(
                    new XElement("Settings",
                        new XElement("BackgroundImagePath", _backgroundImagePath),
                        new XElement("BackgroundImageBlurRadius", _backgroundImageBlurRadius)
                    )
                );
                settings.Save(_settingsFilePath);
            }
            catch (Exception)
            {
                // 保存失败时忽略错误
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var doc = XDocument.Load(_settingsFilePath);
                    var settings = doc.Element("Settings");
                    if (settings != null)
                    {
                        _backgroundImagePath = settings.Element("BackgroundImagePath")?.Value;
                        if (double.TryParse(settings.Element("BackgroundImageBlurRadius")?.Value, out double blurRadius))
                        {
                            _backgroundImageBlurRadius = blurRadius;
                        }
                    }
                }
                else
                {
                    // 如果配置文件不存在，尝试从旧版本迁移设置
                    MigrateFromOldSettings();
                }

                // 验证背景图片路径
                if (string.IsNullOrWhiteSpace(_backgroundImagePath) || !File.Exists(_backgroundImagePath))
                {
                    _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
                }
            }
            catch (Exception)
            {
                // 如果加载失败，使用默认值
                _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
                _backgroundImageBlurRadius = 0;
            }
        }

        private void MigrateFromOldSettings()
        {
            try
            {
                // 从旧的 Properties.Settings 迁移数据
                _backgroundImagePath = Properties.Settings.Default.BackgroundImagePath;
                _backgroundImageBlurRadius = Properties.Settings.Default.BackgroundImageBlurRadius;
                
                // 保存到新的配置文件
                SaveSettings();
                
                // 清理旧的配置
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // 迁移失败时使用默认值
                _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
                _backgroundImageBlurRadius = 0;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 