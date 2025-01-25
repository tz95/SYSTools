﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYSTools.ToolPages
{
    /// <summary>
    /// PeripheralsTools.xaml 的交互逻辑
    /// </summary>
    public partial class PeripheralsTools : Page
    {
        string AppPath = Directory.GetCurrentDirectory();
        string PeripheralsTools_Path = @"Software Package\PeripheralsTools\";       

        public PeripheralsTools()
        {
            InitializeComponent();
        }

        public bool FileExist(string Str_File)
        {
            // 用于查找文件是否存在
            return File.Exists(Str_File);
        }

        public bool DirExist(string Str_Path)
        {
            // 用于查找文件夹是否存在
            return Directory.Exists(Str_Path);
        }

        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DirExist(Path.Combine(AppPath, PeripheralsTools_Path)))
            {
                Directory.CreateDirectory(Path.Combine(AppPath, PeripheralsTools_Path));
            }
        }

        public void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DirExist(Path.Combine(AppPath, PeripheralsTools_Path)))
            {
                Process.Start("explorer.exe", Path.Combine(AppPath, PeripheralsTools_Path));
            }
        }
        public void HandleMouseClick(string ToolName, string ExeName)
        {
            string ExePath = Path.Combine(AppPath, PeripheralsTools_Path, ToolName, ExeName + ".exe");
            if (FileExist(ExePath))
            {
                try
                {
                    Process.Start(ExePath);
                }
                catch (Exception e)
                {
                    iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("请检查程序包内是否存在该工具, 或工具存放位置是否正确 \r\n 或检查杀毒软件是否拦截.", "找不到工具启动文件", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("请检查程序包内是否存在该工具 \r\n 或工具存放位置是否正确", "无法打开该工具", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void HKBTest_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("HKBTest", "HKBTest");
        }

        private void MouseTest_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("MouseTest", "MouseTest");
        }

        private void MouseRate_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("MouseRate", "MouseRate");
        }
    }
}
