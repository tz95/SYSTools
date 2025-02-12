using System;
using System.ComponentModel;
using System.Windows;
using SYSTools.Updater.Services;
using SYSTools.Updater.Utils;

namespace SYSTools.Updater
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ThemeService _themeService;
        private readonly ILogger _logger;
        private UpdateService _updateService;
        private bool _isDarkMode;
        private string _updateTitle;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDarkMode)));
                }
            }
        }

        public string UpdateTitle
        {
            get => _updateTitle;
            set
            {
                if (_updateTitle != value)
                {
                    _updateTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateTitle)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UpdateProgress.IsIndeterminate = true;

            _themeService = new ThemeService();
            _themeService.ThemeChanged += OnThemeChanged;
            IsDarkMode = _themeService.IsDarkMode;

            _logger = new UILogger(LogTextBlock, StatusText, UpdateProgress);
        }

        private void OnThemeChanged(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            InitializeUpdateService();
        }

        private void InitializeUpdateService()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                if (args.Length < 4)
                {
                    ShowError($"参数不足: 需要4个参数，当前只有{args.Length}个参数");
                    LogCommandLineArgs(args);
                    return;
                }

                string zipPath = FileUtils.CleanPath(args[1].Trim('"'));
                string targetPath = FileUtils.CleanPath(args[2].Trim('"'));
                bool isToolkitUpdate = args[3].Trim('"').Equals("toolkit", StringComparison.OrdinalIgnoreCase);

                UpdateTitle = isToolkitUpdate ? "正在更新工具包..." : "正在更新 SYSTools...";

                _logger.Log($"更新类型: {(isToolkitUpdate ? "工具包更新" : "软件更新")}");
                _logger.Log($"更新包路径: {zipPath}");
                _logger.Log($"目标路径: {targetPath}");

                _updateService = new UpdateService(zipPath, targetPath, isToolkitUpdate, _logger);
                StartUpdate();
            }
            catch (Exception ex)
            {
                ShowError($"初始化更新服务失败: {ex.Message}");
            }
        }

        private async void StartUpdate()
        {
            try
            {
                await _updateService.StartUpdate();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void LogCommandLineArgs(string[] args)
        {
            _logger.Log("参数列表:");
            for (int i = 0; i < args.Length; i++)
            {
                _logger.Log($"参数[{i}]: {args[i]}");
            }
        }

        private void ShowError(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowError(message));
                return;
            }
            MessageBox.Show(message, "更新错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _themeService.Cleanup();
        }
    }
} 