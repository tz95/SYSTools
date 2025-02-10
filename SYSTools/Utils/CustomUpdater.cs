using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.IO.Compression;
using System.Linq;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System.Windows.Controls;

namespace SYSTools.Utils
{
    public class CustomUpdater
    {
        private static readonly HttpClient Client = new HttpClient();
        
        public enum UpdateFileType
        {
            Executable,
            ZipPackage,
            Unknown
        }
        
        public class UpdateInfo
        {
            public Version Version { get; set; }
            public string DownloadUrl { get; set; }
            public string ReleaseNotes { get; set; }
            public long FileSize { get; set; }
            public UpdateFileType FileType { get; set; }
        }

        // 检查文件类型的方法
        private static UpdateFileType DetectFileType(byte[] fileHeader)
        {
            // ZIP文件头: 50 4B 03 04
            byte[] zipSignature = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
            // EXE文件头: 4D 5A
            byte[] exeSignature = new byte[] { 0x4D, 0x5A };

            if (fileHeader.Take(4).SequenceEqual(zipSignature))
                return UpdateFileType.ZipPackage;
            else if (fileHeader.Take(2).SequenceEqual(exeSignature))
                return UpdateFileType.Executable;
            
            return UpdateFileType.Unknown;
        }

        public static async Task<UpdateInfo> CheckForUpdate(string versionUrl)
        {
            try
            {
                var response = await Client.GetAsync(versionUrl);
                response.EnsureSuccessStatusCode();
                var updateInfo = await response.Content.ReadAsStringAsync();
                
                // 现在只需要: 版本号|下载地址|更新说明|文件大小
                var parts = updateInfo.Split('|');
                
                // 获取文件头进行类型检测
                var fileResponse = await Client.GetAsync(parts[1], HttpCompletionOption.ResponseHeadersRead);
                var buffer = new byte[4];
                using (var stream = await fileResponse.Content.ReadAsStreamAsync())
                {
                    await stream.ReadAsync(buffer, 0, 4);
                }

                return new UpdateInfo
                {
                    Version = new Version(parts[0]),
                    DownloadUrl = parts[1],
                    ReleaseNotes = parts[2],
                    FileSize = long.Parse(parts[3]),
                    FileType = DetectFileType(buffer)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("检查更新失败: " + ex.Message);
            }
        }

        public static async Task ShowUpdateDialog(UpdateInfo updateInfo)
        {
            if (updateInfo.FileType == UpdateFileType.Unknown)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    "无法识别更新文件类型，更新已取消", 
                    "更新错误",
                    MessageBoxButton.OK,
                    SegoeFluentIcons.Error
                );
                return;
            }

            // 计算文件大小显示
            string sizeDisplay;
            if (updateInfo.FileSize < 1024) // 小于1KB
            {
                sizeDisplay = $"{updateInfo.FileSize} B";
            }
            else if (updateInfo.FileSize < 1024 * 1024) // 小于1MB
            {
                sizeDisplay = $"{updateInfo.FileSize / 1024.0:F2} KB";
            }
            else if (updateInfo.FileSize < 1024 * 1024 * 1024) // 小于1GB
            {
                sizeDisplay = $"{updateInfo.FileSize / 1024.0 / 1024.0:F2} MB";
            }
            else // GB及以上
            {
                sizeDisplay = $"{updateInfo.FileSize / 1024.0 / 1024.0 / 1024.0:F2} GB";
            }

            var dialog = new ContentDialog
            {
                Title = "发现新版本",
                Content = $"当前有新版本 {updateInfo.Version} 可用\n" +
                         $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                         $"更新说明:\n" +
                         $"{updateInfo.ReleaseNotes}\n" +
                         $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                         $"文件大小: {sizeDisplay}\n" +
                         $"更新类型: {(updateInfo.FileType == UpdateFileType.Executable ? "可执行文件" : "压缩包")}",
                PrimaryButtonText = "立即更新",
                CloseButtonText = "取消"
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await DownloadAndInstall(updateInfo);
            }
        }

        private static async Task DownloadAndInstall(UpdateInfo updateInfo)
        {
            string tempFileName = updateInfo.FileType == UpdateFileType.Executable ? 
                "SYSTools_Update.exe" : "SYSTools_Update.zip";
            var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);
            
            var progressGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var progressBar = new iNKORE.UI.WPF.Modern.Controls.ProgressBar
            {
                IsIndeterminate = false,
                Minimum = 0,
                Maximum = 100,
                Height = 4,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var statusText = new TextBlock
            {
                Text = "准备下载...",
                TextWrapping = TextWrapping.Wrap
            };

            progressGrid.Children.Add(progressBar);
            progressGrid.Children.Add(statusText);
            Grid.SetRow(statusText, 1);

            var progressDialog = new ContentDialog
            {
                Title = "正在下载更新",
                Content = progressGrid
            };

            try
            {
                using (var webClient = new WebClient())
                {
                    var lastBytes = 0L;
                    var lastTime = DateTime.Now;
                    var updateInterval = TimeSpan.FromSeconds(1);

                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                        
                        var now = DateTime.Now;
                        if (now - lastTime >= updateInterval)
                        {
                            var bytesChange = e.BytesReceived - lastBytes;
                            var speed = bytesChange / (now - lastTime).TotalSeconds;
                            
                            string speedText;
                            if (speed >= 1024 * 1024) // MB/s
                            {
                                speedText = $"{speed / 1024 / 1024:F2} MB/s";
                            }
                            else if (speed >= 1024) // KB/s
                            {
                                speedText = $"{speed / 1024:F2} KB/s";
                            }
                            else // B/s
                            {
                                speedText = $"{speed:F0} B/s";
                            }

                            statusText.Text = $"已下载: {FormatFileSize(e.BytesReceived)} / {FormatFileSize(e.TotalBytesToReceive)} ({e.ProgressPercentage}%)\n" +
                                            $"下载速度: {speedText}";

                            lastBytes = e.BytesReceived;
                            lastTime = now;
                        }
                    };

                    webClient.DownloadFileCompleted += async (s, e) =>
                    {
                        progressDialog.Hide();
                        if (e.Error == null && !e.Cancelled)
                        {
                            // 下载完成后再次验证文件类型
                            byte[] fileHeader = new byte[4];
                            using (var fs = File.OpenRead(tempPath))
                            {
                                fs.Read(fileHeader, 0, 4);
                            }
                            var actualFileType = DetectFileType(fileHeader);

                            if (actualFileType != updateInfo.FileType)
                            {
                                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                                    "更新文件类型验证失败，更新已取消", 
                                    "更新错误",
                                    MessageBoxButton.OK,
                                    SegoeFluentIcons.Error
                                );
                                return;
                            }

                            if (updateInfo.FileType == UpdateFileType.Executable)
                            {
                                Process.Start(tempPath);
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                await InstallZipUpdate(tempPath);
                            }
                        }
                    };

                    _ = progressDialog.ShowAsync();
                    await webClient.DownloadFileTaskAsync(new Uri(updateInfo.DownloadUrl), tempPath);
                }
            }
            catch (Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    "下载更新失败: " + ex.Message, 
                    "更新错误", 
                    MessageBoxButton.OK, 
                    SegoeFluentIcons.Error
                );
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:F2} {sizes[order]}";
        }

        private static async Task InstallZipUpdate(string zipPath)
        {
            try
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                string updaterPath = Path.Combine(appPath, "SYSTools.Updater.exe");
                string tempUpdaterPath = Path.Combine(Path.GetTempPath(), "SYSTools.Updater.exe");

                // 如果是工具包更新，确保目标目录存在
                if (zipPath.Contains("Tools_Update"))
                {
                    string softwarePackagePath = Path.Combine(appPath, "Software Package");
                    if (!Directory.Exists(softwarePackagePath))
                    {
                        Directory.CreateDirectory(softwarePackagePath);
                    }
                }

                // 复制更新器到临时目录
                if (File.Exists(updaterPath))
                {
                    File.Copy(updaterPath, tempUpdaterPath, true);
                }
                else
                {
                    iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                        "找不到更新器程序", 
                        "更新错误",
                        MessageBoxButton.OK,
                        SegoeFluentIcons.Error
                    );
                    return;
                }

                // 清理和准备参数
                string cleanZipPath = zipPath.Trim('"').TrimEnd('\\');
                string cleanAppPath = appPath.Trim('"').TrimEnd('\\');
                string updateType = cleanZipPath.Contains("Tools_Update") ? "toolkit" : "software";

                // 构建参数数组并连接
                string[] arguments = new[]
                {
                    $"\"{cleanZipPath}\"",
                    $"\"{cleanAppPath}\"",
                    updateType
                };

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = tempUpdaterPath,
                    Arguments = string.Join(" ", arguments),
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    "启动更新失败: " + ex.Message, 
                    "更新错误",
                    MessageBoxButton.OK,
                    SegoeFluentIcons.Error
                );
            }
        }
    }
} 