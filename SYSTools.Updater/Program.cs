using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SYSTools.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("参数不足");
                return;
            }

            try
            {
                // 清理路径中的非法字符
                string zipPath = CleanPath(args[0].Trim('"'));
                string targetPath = CleanPath(args[1].Trim('"'));
                string processName = "SYSTools";

                Console.WriteLine($"更新包路径: {zipPath}");
                Console.WriteLine($"目标路径: {targetPath}");

                if (!File.Exists(zipPath))
                {
                    Console.WriteLine("更新包文件不存在");
                    return;
                }

                if (!Directory.Exists(targetPath))
                {
                    Console.WriteLine("目标目录不存在");
                    return;
                }

                // 等待主程序退出
                Console.WriteLine("等待主程序退出...");
                Thread.Sleep(1000);
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.WaitForExit();
                }

                // 创建备份
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string backupPath = Path.Combine(targetPath, $"backup_{timestamp}");
                Console.WriteLine($"创建备份: {backupPath}");

                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                // 备份文件
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
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"备份文件失败: {file}, 错误: {ex.Message}");
                    }
                }

                // 解压更新文件
                Console.WriteLine("开始解压更新文件...");
                using (var archive = ZipFile.OpenRead(zipPath))
                {
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
                                Console.WriteLine($"已解压: {entry.FullName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"解压文件失败: {entry.FullName}, 错误: {ex.Message}");
                        }
                    }
                }

                // 删除临时文件
                try
                {
                    File.Delete(zipPath);
                    Console.WriteLine("已删除临时文件");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"删除临时文件失败: {ex.Message}");
                }

                // 启动主程序
                string mainExe = Path.Combine(targetPath, "SYSTools.exe");
                Console.WriteLine("启动主程序...");
                Process.Start(mainExe);
                Console.WriteLine("更新完成!");
            }
            catch (Exception ex)
            {
                string errorLog = $"更新失败: {ex}\n\n参数:\n{string.Join("\n", args)}";
                File.WriteAllText("update_error.log", errorLog);
                Console.WriteLine(errorLog);
                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey();
            }
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