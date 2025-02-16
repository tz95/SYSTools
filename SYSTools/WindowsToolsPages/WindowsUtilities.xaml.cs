﻿using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SYSTools.WindowsToolsPages
{
    /// <summary>
    /// WindowsToolsPages.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsUtilities : System.Windows.Controls.Page
    {
        private bool isInitializing = true;

        public WindowsUtilities()
        {
            InitializeComponent();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            // 初始化文件夹选项状态
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
            {
                if (key != null)
                {
                    ShowFileExt.IsChecked = (int)key.GetValue("HideFileExt", 1) == 0;
                    ShowHidden.IsChecked = (int)key.GetValue("Hidden", 0) == 1;
                    ShowSuper.IsChecked = (int)key.GetValue("ShowSuperHidden", 0) == 1;
                    HideArrow.IsChecked = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons")?.GetValue("29") != null;
                    byte[] linkValue = (byte[])Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer")?.GetValue("link");
                    HideText.IsChecked = linkValue != null && linkValue.Length == 4 &&
                        linkValue[0] == 0x00 && linkValue[1] == 0x00 &&
                        linkValue[2] == 0x00 && linkValue[3] == 0x00;
                    HideUAC.IsChecked = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons")?.GetValue("77") != null;
                }
            }

            // 初始化右键菜单风格
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"))
            {
                MenuStyle.SelectedIndex = (key == null) ? 1 : 0;
            }

            // 初始化资源管理器默认打开位置
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
            {
                if (key != null)
                {
                    // LaunchTo=2 时选择"快速访问"，LaunchTo=1 时选择"此电脑"
                    ExplorerDefault.SelectedIndex = (int)key.GetValue("LaunchTo", 1) == 2 ? 1 : 0;
                }
            }

            // 初始化电源计划选择
            InitializePowerConfig();

            isInitializing = false;
        }

        private void ShowFileExt_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
            {
                if (key != null)
                {
                    key.SetValue("HideFileExt", ShowFileExt.IsChecked == true ? 0 : 1, RegistryValueKind.DWord);
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void ShowHidden_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
            {
                if (key != null)
                {
                    key.SetValue("Hidden", ShowHidden.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void ShowSuper_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
            {
                if (key != null)
                {
                    key.SetValue("ShowSuperHidden", ShowSuper.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void HideArrow_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", true))
            {
                if (key != null)
                {
                    if (HideArrow.IsChecked == true)
                    {
                        key.SetValue("29", "%systemroot%\\system32\\imageres.dll,197", RegistryValueKind.String);
                    }
                    else
                    {
                        key.DeleteValue("29", false);
                    }
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void HideText_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", true))
            {
                if (key != null)
                {
                    if (HideText.IsChecked == true)
                    {
                        key.SetValue("link", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                    }
                    else
                    {
                        key.SetValue("link", new byte[] { 0x19, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                    }
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void HideUAC_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", true))
            {
                if (key != null)
                {
                    if (HideUAC.IsChecked == true)
                    {
                        key.SetValue("77", "%systemroot%\\system32\\imageres.dll,197", RegistryValueKind.String);
                    }
                    else
                    {
                        key.DeleteValue("77", false);
                    }
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void MenuStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", true))
            {
                if (key != null)
                {
                    if (MenuStyle.SelectedIndex == 0) // Win10风格
                    {
                        key.SetValue("", "", RegistryValueKind.String);
                    }
                    else // Win11风格
                    {
                        Registry.CurrentUser.DeleteSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", false);
                    }
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        private void ExplorerDefault_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
            {
                if (key != null)
                {
                    // 选择"此电脑"(SelectedIndex=0)时设置LaunchTo=1，选择"快速访问"(SelectedIndex=1)时设置LaunchTo=2
                    key.SetValue("LaunchTo", ExplorerDefault.SelectedIndex == 0 ? 1 : 2, RegistryValueKind.DWord);
                    ShowMessage("设置已更改，需要重启资源管理器生效");
                }
            }
        }

        //快捷启动分类
        private void CMD_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd") { WorkingDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) });
        }

        private void PowerShell_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("powershell") { WorkingDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) });
        }

        private void Regedit_Click(object sender, RoutedEventArgs e)
        {
            //注册表
            Process.Start("regedit");
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            //控制面板
            Process.Start("control");
        }

        private void compmgmt_Click(object sender, RoutedEventArgs e)
        {
            //计算机管理
            Process.Start("compmgmt.msc");
        }

        private void Eventvwr_Click(object sender, RoutedEventArgs e)
        {
            //事件查看器
            Process.Start("eventvwr");
        }

        private void Devmgmt_Click(object sender, RoutedEventArgs e)
        {
            //设备管理器
            Process.Start("devmgmt.msc");
        }

        private void Gpedit_Click(object sender, RoutedEventArgs e)
        {
            //组策略
            Process.Start("gpedit.msc");
        }
        private void Taskschd_Click(object sender, RoutedEventArgs e)
        {
            //计划任务
            Process.Start("taskschd.msc");
        }

        private void GodMode_Click(object sender, RoutedEventArgs e)
        {
            //上帝模式
            Process.Start("shell:::{ED7BA470-8E54-465E-825C-99712043E01C}");
        }

        private void Winver_Click(object sender, RoutedEventArgs e)
        {
            //关于Windows
            Process.Start("winver");
        }

        private void Explorer_Re(object sender, RoutedEventArgs e)
        {
            // 重启资源管理器
            Process[] processes = Process.GetProcessesByName("explorer");
            foreach (Process process in processes)
            {
                process.Kill();
            }
            Process.Start("explorer.exe");
        }
        //网络工具分类
        private void ClearDNS_Click(object sender, RoutedEventArgs e)
        {
            string output = RunCommand("ipconfig /flushdns");
            ShowMessage(output);
        }

        private void ResetWS_Click(object sender, RoutedEventArgs e)
        {
            string output = RunCommand("netsh winsock reset");
            ShowMessage(output);
        }

        private void ResetWS_LSP_Click(object sender, RoutedEventArgs e)
        {
            string output = RunCommand("netsh winsock reset catalog");
            ShowMessage(output);
        }

        private void ResetTCP_Click(object sender, RoutedEventArgs e)
        {
            string output = RunCommand("netsh int ip reset");
            ShowMessage(output);
        }

        // 电源计划
        private void InitializePowerConfig()
        {
            string output = RunCommand("powercfg /list");
            string[] lines = output.Split('\n');
            string currentGuid = RunCommand("powercfg /getactivescheme").Split('\n')[0].Trim();

            foreach (string line in lines)
            {
                if (line.Contains("电源方案 GUID"))
                {
                    if (line.Contains("(节能)") || line.Contains("最佳能效"))
                    {
                        if (line.Contains(currentGuid))
                            PowerConfig.SelectedIndex = 0;
                    }
                    else if (line.Contains("(平衡)") || line.Contains("平衡"))
                    {
                        if (line.Contains(currentGuid))
                            PowerConfig.SelectedIndex = 1;
                    }
                    else if (line.Contains("(高性能)") || line.Contains("最佳性能"))
                    {
                        if (line.Contains(currentGuid))
                            PowerConfig.SelectedIndex = 2;
                    }
                    else if (line.Contains("卓越性能"))
                    {
                        if (line.Contains(currentGuid))
                            PowerConfig.SelectedIndex = 3;
                    }
                }
            }
        }

        private void PowerConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            string scheme = "";
            switch (PowerConfig.SelectedIndex)
            {
                case 0: // 最佳能效
                    scheme = "a1841308-3541-4fab-bc81-f71556f20b4a";
                    break;
                case 1: // 平衡
                    scheme = "381b4222-f694-41f0-9685-ff5bb260df2e";
                    break;
                case 2: // 最佳性能
                    scheme = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
                    break;
                case 3: // 卓越性能
                    // 先创建卓越性能方案
                    RunCommand("powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                    scheme = "e9a42b02-d5df-448d-aa00-03f14749eb61";
                    break;
            }

            if (!string.IsNullOrEmpty(scheme))
            {
                string output = RunCommand($"powercfg /setactive {scheme}");
                ShowMessage("电源计划已更改: " + output);
            }
        }




        // cmd命令执行
        static string RunCommand(string command)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c chcp 65001 > nul && {command}", // 切换到UTF-8代码页
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                }
            };
            
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            return string.IsNullOrEmpty(error) ? output.Trim() : error.Trim();
        }

        // 窗口提示
        private async void ShowMessage(string message)
        {
            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 400, // 设置最大高度
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };

            scrollViewer.Content = textBlock;

            ContentDialog dialog = new ContentDialog
            {
                Title = "命令执行结果",
                Content = scrollViewer,
                CloseButtonText = "确定"
            };

            await dialog.ShowAsync();
        }
    }
}

