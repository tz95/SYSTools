using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using Microsoft.Win32;
using System.ComponentModel;

namespace SYSTools.Updater
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string zipPath;
        private string targetPath;
        private string backupPath;
        private bool _isDarkMode;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            UpdateProgress.IsIndeterminate = true;
            
            // 检测系统主题
            IsDarkMode = IsSystemDarkMode();
            ApplyTheme(IsDarkMode);

            // 监听系统主题变化
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            StartUpdate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                IsDarkMode = IsSystemDarkMode();
                Dispatcher.Invoke(() => ApplyTheme(IsDarkMode));
            }
        }

        private bool IsSystemDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            return (int)value == 0;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private void ApplyTheme(bool isDark)
        {
            IsDarkMode = isDark;
        }

        private async void StartUpdate()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    ShowError("参数不足");
                    return;
                }

                zipPath = CleanPath(args[1].Trim('"'));
                targetPath = CleanPath(args[2].Trim('"'));

                LogMessage($"更新包路径: {zipPath}");
                LogMessage($"目标路径: {targetPath}");

                if (!File.Exists(zipPath))
                {
                    ShowError("更新包文件不存在");
                    return;
                }

                if (!Directory.Exists(targetPath))
                {
                    ShowError("目标目录不存在");
                    return;
                }

                await Task.Run(() => WaitForMainProcess());
                await CreateBackup();
                await ExtractUpdate();
                await CleanupAndStart();
            }
            catch (Exception ex)
            {
                ShowError($"更新失败: {ex.Message}");
                File.WriteAllText("update_error.log", ex.ToString());
            }
        }

        private void WaitForMainProcess()
        {
            UpdateStatus("等待主程序退出...");
            System.Threading.Thread.Sleep(1000);
            foreach (var process in Process.GetProcessesByName("SYSTools"))
            {
                process.WaitForExit();
            }
        }

        private async Task CreateBackup()
        {
            UpdateStatus("正在创建备份...");
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            backupPath = Path.Combine(targetPath, $"backup_{timestamp}");
            LogMessage($"创建备份: {backupPath}");

            await Task.Run(() =>
            {
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                int totalFiles = Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories).Length;
                int processedFiles = 0;

                foreach (string file in Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (!file.Contains("backup_"))
                        {
                            string relativePath = GetRelativePath(file, targetPath);
                            string backupFile = Path.Combine(backupPath, relativePath);
                            string backupDir = Path.GetDirectoryName(backupFile);

                            if (!Directory.Exists(backupDir))
                                Directory.CreateDirectory(backupDir);

                            File.Copy(file, backupFile, true);
                            processedFiles++;
                            UpdateProgressValue((double)processedFiles / totalFiles * 100);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"备份文件失败: {file}, 错误: {ex.Message}");
                    }
                }
            });
        }

        private async Task ExtractUpdate()
        {
            UpdateStatus("正在更新文件...");
            UpdateProgressValue(0);

            await Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    int totalEntries = archive.Entries.Count;
                    int processedEntries = 0;
                    long totalSize = archive.Entries.Sum(entry => entry.Length);
                    long processedSize = 0;

                    foreach (var entry in archive.Entries)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                string destinationPath = Path.Combine(targetPath, CleanPath(entry.FullName));
                                string destinationDir = Path.GetDirectoryName(destinationPath);

                                if (!Directory.Exists(destinationDir))
                                    Directory.CreateDirectory(destinationDir);

                                entry.ExtractToFile(destinationPath, true);
                                
                                processedEntries++;
                                processedSize += entry.Length;
                                
                                // 同时考虑文件数量和大小的进度
                                double filesProgress = (double)processedEntries / totalEntries;
                                double sizeProgress = (double)processedSize / totalSize;
                                double totalProgress = (filesProgress + sizeProgress) * 50; // 平均两种进度

                                UpdateProgressValue(totalProgress);
                                LogMessage($"已更新: {entry.FullName} ({processedEntries}/{totalEntries})");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"更新文件失败: {entry.FullName}, 错误: {ex.Message}");
                        }
                    }
                    
                    // 确保进度条到达100%
                    UpdateProgressValue(100);
                }
            });
        }

        private async Task CleanupAndStart()
        {
            UpdateStatus("正在清理临时文件...");
            try
            {
                File.Delete(zipPath);
                LogMessage("已删除临时文件");

                string mainExe = Path.Combine(targetPath, "SYSTools.exe");
                UpdateStatus("正在启动程序...");
                
                var mainProcess = Process.Start(mainExe);
                await Task.Delay(3000);

                if (!mainProcess.HasExited)
                {
                    UpdateStatus("更新完成，正在清理备份...");
                    if (Directory.Exists(backupPath))
                    {
                        Directory.Delete(backupPath, true);
                        LogMessage("备份文件清理完成");
                    }
                    await Task.Delay(1000);
                    Close();
                }
                else
                {
                    ShowError("程序启动失败，保留备份文件");
                }
            }
            catch (Exception ex)
            {
                ShowError($"清理过程出错: {ex.Message}");
            }
        }

        private void UpdateStatus(string status)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateStatus(status));
                return;
            }
            StatusText.Text = status;
            LogMessage(status);
        }

        private void UpdateProgressValue(double value)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateProgressValue(value));
                return;
            }
            
            // 确保进度值在有效范围内
            value = Math.Max(0, Math.Min(100, value));
            
            UpdateProgress.IsIndeterminate = false;
            
            // 使用动画使进度条更新更平滑
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = value,
                Duration = TimeSpan.FromMilliseconds(200),
                FillBehavior = System.Windows.Media.Animation.FillBehavior.Stop
            };
            
            animation.Completed += (s, e) => UpdateProgress.Value = value;
            UpdateProgress.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        private void LogMessage(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => LogMessage(message));
                return;
            }
            LogTextBlock.Text += message + "\n";
            
            // 使用 Dispatcher 确保在文本更新后滚动
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var scrollViewer = (ScrollViewer)LogTextBlock.Parent;
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
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

        private static string CleanPath(string path)
        {
            // 移除非法字符
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(path, invalidRegStr, "_");
        }

        private static string GetRelativePath(string fullPath, string basePath)
        {
            try
            {
                // 确保路径以目录分隔符结束
                if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basePath += Path.DirectorySeparatorChar;

                Uri baseUri = new Uri(basePath);
                Uri fullUri = new Uri(fullPath);
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

                // 将 URI 格式转换为本地路径格式
                return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
            }
            catch
            {
                // 如果 URI 方法失败，使用字符串处理方法
                return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
            }
        }
    }
} 