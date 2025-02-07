using System;
using System.ComponentModel;
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
        private double _backgroundImageOpacity;
        private bool _isBackgroundEnabled;
        private int _themeMode;
        private string _language;

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

        public double BackgroundImageOpacity
        {
            get => _backgroundImageOpacity;
            set
            {
                if (_backgroundImageOpacity != value)
                {
                    _backgroundImageOpacity = value;
                    SaveSettings();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActualOpacity));
                }
            }
        }

        public double ActualOpacity => _backgroundImageOpacity / 100.0;

        public bool IsBackgroundEnabled
        {
            get => _isBackgroundEnabled;
            set
            {
                if (_isBackgroundEnabled != value)
                {
                    _isBackgroundEnabled = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public int ThemeMode
        {
            get => _themeMode;
            set
            {
                if (_themeMode != value)
                {
                    _themeMode = value;
                    SaveSettings();
                    OnPropertyChanged();
                    ApplyTheme();
                }
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        private void ApplyTheme()
        {
            switch (_themeMode)
            {
                case 0: // 跟随系统
                    iNKORE.UI.WPF.Modern.ThemeManager.Current.ApplicationTheme = null;
                    break;
                case 1: // 深色
                    iNKORE.UI.WPF.Modern.ThemeManager.Current.ApplicationTheme = 
                        iNKORE.UI.WPF.Modern.ApplicationTheme.Dark;
                    break;
                case 2: // 浅色
                    iNKORE.UI.WPF.Modern.ThemeManager.Current.ApplicationTheme = 
                        iNKORE.UI.WPF.Modern.ApplicationTheme.Light;
                    break;
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new XDocument(
                    new XElement("Settings",
                        new XElement("BackgroundImagePath", _backgroundImagePath),
                        new XElement("BackgroundImageBlurRadius", _backgroundImageBlurRadius),
                        new XElement("BackgroundImageOpacity", _backgroundImageOpacity),
                        new XElement("IsBackgroundEnabled", _isBackgroundEnabled),
                        new XElement("ThemeMode", _themeMode),
                        new XElement("Language", _language)
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
                        if (double.TryParse(settings.Element("BackgroundImageOpacity")?.Value, out double opacity))
                        {
                            _backgroundImageOpacity = opacity;
                        }
                        if (bool.TryParse(settings.Element("IsBackgroundEnabled")?.Value, out bool isEnabled))
                        {
                            _isBackgroundEnabled = isEnabled;
                        }
                        if (int.TryParse(settings.Element("ThemeMode")?.Value, out int themeMode))
                        {
                            _themeMode = themeMode;
                        }
                        _language = settings.Element("Language")?.Value ?? "zh-CN";
                    }
                }

                // 验证背景图片路径
                if (string.IsNullOrWhiteSpace(_backgroundImagePath) || !File.Exists(_backgroundImagePath))
                {
                    _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
                }

                // 应用主题设置
                ApplyTheme();
            }
            catch (Exception)
            {
                // 如果加载失败，使用默认值
                _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
                _backgroundImageBlurRadius = 0;
                _backgroundImageOpacity = 100.0;
                _isBackgroundEnabled = false;
                _themeMode = 0; // 默认跟随系统 无需应用ApplyTheme变更程序主题
                _language = "zh-CN";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 