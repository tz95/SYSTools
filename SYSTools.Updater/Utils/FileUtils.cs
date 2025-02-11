using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using SYSTools.Updater.Services;

namespace SYSTools.Updater.Utils
{
    public static class FileUtils
    {
        public static void CreateBackup(string sourcePath, string backupPath, ILogger logger)
        {
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            int totalFiles = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length;
            int processedFiles = 0;

            foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    if (!file.Contains("backup_"))
                    {
                        string relativePath = GetRelativePath(file, sourcePath);
                        string backupFile = Path.Combine(backupPath, relativePath);
                        string backupDir = Path.GetDirectoryName(backupFile);

                        if (!Directory.Exists(backupDir))
                            Directory.CreateDirectory(backupDir);

                        File.Copy(file, backupFile, true);
                        processedFiles++;
                        logger.UpdateProgress((double)processedFiles / totalFiles * 100);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"备份文件失败: {file}, 错误: {ex.Message}");
                }
            }
        }

        public static void ExtractUpdate(string zipPath, string targetPath, ILogger logger)
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

                            if (File.Exists(destinationPath))
                            {
                                ForceDeleteFile(destinationPath);
                            }

                            entry.ExtractToFile(destinationPath, true);
                            
                            processedEntries++;
                            processedSize += entry.Length;
                            
                            double progress = ((double)processedSize / totalSize) * 100;
                            logger.UpdateProgress(progress);
                            logger.Log($"已更新: {entry.FullName} ({processedEntries}/{totalEntries})");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"更新文件失败: {entry.FullName}, 错误: {ex.Message}");
                    }
                }
            }
        }

        public static void CleanHistoryBackups(string targetPath, ILogger logger)
        {
            try
            {
                var backupFolders = Directory.GetDirectories(targetPath, "backup_*");
                foreach (var folder in backupFolders)
                {
                    try
                    {
                        Directory.Delete(folder, true);
                        logger.Log($"清理历史备份: {Path.GetFileName(folder)}");
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"清理历史备份失败: {Path.GetFileName(folder)}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"查找历史备份失败: {ex.Message}");
            }
        }

        private static void ForceDeleteFile(string path)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        foreach (var module in process.Modules.Cast<ProcessModule>())
                        {
                            if (module.FileName.Equals(path, StringComparison.OrdinalIgnoreCase))
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                                break;
                            }
                        }
                    }
                    catch { }
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        public static string CleanPath(string path)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(path, invalidRegStr, "_");
        }

        private static string GetRelativePath(string fullPath, string basePath)
        {
            try
            {
                if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basePath += Path.DirectorySeparatorChar;

                Uri baseUri = new Uri(basePath);
                Uri fullUri = new Uri(fullPath);
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

                return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
            }
            catch
            {
                return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
            }
        }
    }
} 