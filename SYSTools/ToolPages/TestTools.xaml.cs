﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYSTools.ToolPages
{
    /// <summary>
    /// TestTools.xaml 的交互逻辑
    /// </summary>
    public partial class TestTools : Page
    {
        string AppPath = Directory.GetCurrentDirectory();
        string TestTools_Path = @"Software Package\TestTools\";

        public TestTools()
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
            if (!DirExist(Path.Combine(AppPath, TestTools_Path)))
            {
                Directory.CreateDirectory(Path.Combine(AppPath, TestTools_Path));
            }
        }

        public void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DirExist(Path.Combine(AppPath, TestTools_Path)))
            {
                //Process.Start("explorer.exe", AppPath + @"\Software Package\TestTools\");
                Process.Start("explorer.exe", Path.Combine(AppPath, TestTools_Path));
            }
        }
        public void HandleMouseClick(string ToolName, string ExeName)
        {
            string ExePath = Path.Combine(AppPath, TestTools_Path, ToolName, ExeName + ".exe");
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

        private void Prime95_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("Prime95", "Prime95");
        }

        private void IntelBurnTest_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("IntelBurnTest", "IntelBurnTest");
        }

        private void FurMark_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("FurMark", "FurMark");
        }

        private void MemTest_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("MemTest", "MemTest");
        }
    }
}
