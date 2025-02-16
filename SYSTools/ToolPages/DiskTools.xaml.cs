﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYSTools.ToolPages
{
    /// <summary>
    /// DiskTools.xaml 的交互逻辑
    /// </summary>
    public partial class DiskTools : Page
    {
        string AppPath = Directory.GetCurrentDirectory();
        string DiskTools_Path = @"Software Package\DiskTools\";

        public DiskTools()
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
            if (!DirExist(Path.Combine(AppPath, DiskTools_Path)))
            {
                Directory.CreateDirectory(Path.Combine(AppPath, DiskTools_Path));
            }
        }

        public void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DirExist(Path.Combine(AppPath, DiskTools_Path)))
            {
                Process.Start("explorer.exe", Path.Combine(AppPath, DiskTools_Path));
            }
        }
        public void HandleMouseClick(string ToolName, string ExeName)
        {
            string ExePath = Path.Combine(AppPath, DiskTools_Path, ToolName, ExeName + ".exe");
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

        private void AS_SSD_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("AS SSD Benchmark", "AS SSD Benchmark");
        }

        private void CrystalDiskInfo_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("CrystalDiskInfo", "CrystalDiskInfo");
        }

        private void CrystalDiskMark_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("CrystalDiskMark", "CrystalDiskMark");
        }

        private void DiskBenchmark_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("DiskBenchmark", "DiskBenchmark");
        }

        private void DiskGenius_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("DiskGenius", "DiskGenius");
        }

        private void HDTune_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("HDTune", "HDTune");
        }

        private void LLFTOOL_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("LLFTOOL", "LLFTOOL");
        }

        private void PartAssist_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("PartAssist", "PartAssist");
        }

        private void SSDZ_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("SSDZ", "SSDZ");
        }

        private void Victoria_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("Victoria", "Victoria");
        }

        private void H2TestW_Click(object sender, RoutedEventArgs e)
        {
            HandleMouseClick("H2TestW", "H2TestW");
        }

    }
}
