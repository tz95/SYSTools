using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;

namespace SYSTools.Model
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());
        public static AppSettings Instance => _instance.Value;

        private string _backgroundImagePath;
        private double _backgroundImageBlurRadius;

        public event PropertyChangedEventHandler PropertyChanged;

        private AppSettings()
        {
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
                    Properties.Settings.Default.BackgroundImagePath = value;
                    Properties.Settings.Default.Save();
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
                    Properties.Settings.Default.BackgroundImageBlurRadius = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public void LoadSettings()
        {
            if (!ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Save();
            }

            _backgroundImagePath = Properties.Settings.Default.BackgroundImagePath;
            _backgroundImageBlurRadius = Properties.Settings.Default.BackgroundImageBlurRadius;

            if (string.IsNullOrWhiteSpace(_backgroundImagePath) || !File.Exists(_backgroundImagePath))
            {
                _backgroundImagePath = "pack://application:,,,/Resources/NoBackImage.png";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 