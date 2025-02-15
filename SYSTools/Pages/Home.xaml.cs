﻿using System;
using System.Diagnostics;
using System.Management;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace SYSTools.Pages
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {
        static readonly HttpClient client = new HttpClient();
        DateTime UPTime = DateTime.Now.AddMilliseconds(-(Environment.TickCount));
        private List<string> notices = new List<string>();
        private int currentNoticeIndex = 0;
        private DispatcherTimer noticeTimer;

        public Home()
        {
            InitializeComponent();
            
            // 初始化公告计时器
            noticeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            noticeTimer.Tick += NoticeTimer_Tick;
            
            // 注册Unloaded事件
            this.Unloaded += Home_Unloaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OpenTime.Text = UPTime.ToLongDateString();

            //欢迎头部文本
            Home_SP_Tip.ToolTip = Properties.Lang.ResourceManager.GetString("Hello", System.Globalization.CultureInfo.CurrentUICulture) + Convert.ToChar(32) + Environment.UserName;


            //一言卡片获取
            try
            {
                System.Net.Http.HttpResponseMessage response = await client.GetAsync("https://v1.hitokoto.cn/?c=b&c=a&encode=text");
                response.EnsureSuccessStatusCode();
                string webCode = await response.Content.ReadAsStringAsync();
                Hitokoto.Text = webCode;
            }
            catch (Exception)
            {
                Hitokoto.Text = "！- " + Properties.Lang.ResourceManager.GetString("NetError", System.Globalization.CultureInfo.CurrentUICulture) + " - ！";
            }

            if (Hitokoto.Text == "！- " + Properties.Lang.ResourceManager.GetString("NetError", System.Globalization.CultureInfo.CurrentUICulture) + " - ！")
            {
                IPv4.Text = "!";
                IPv6.Text = "!";
            }

            // 获取Windows系统版本与版本号
            ManagementObjectSearcher OpSystem = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectCollection OpS = OpSystem.Get();
            foreach (ManagementObject OpSys in OpS)
            {
                Windows_Name.Text = OpSys.GetPropertyValue("Caption").ToString();
                Windows_Version.Text = OpSys.GetPropertyValue("Version").ToString();
            }

            // 获取公告
            try
            {
                // 根据当前语言选择公告URL
                string noticeUrl = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("zh") 
                    ? "http://systools.hksstudio.work/PublicNotice"          // 中文公告
                    : "http://systools.hksstudio.work/PublicNotice_EN";      // 英文公告

                HttpResponseMessage response = await client.GetAsync(noticeUrl);
                response.EnsureSuccessStatusCode();
                string noticeContent = await response.Content.ReadAsStringAsync();
                
                // 按行分割公告内容
                notices = noticeContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(n => n.Trim())
                                     .Where(n => !string.IsNullOrEmpty(n))
                                     .ToList();

                if (notices.Count > 0)
                {
                    string firstNotice = notices[0];
                    PublicNotice.Text = firstNotice;
                    
                    if (notices.Count > 1)
                    {
                        // 设置初始间隔
                        noticeTimer.Interval = firstNotice.Length > 15 ? 
                            TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(3);
                        noticeTimer.Start();
                    }
                }
                else
                {
                    PublicNotice.Text = Properties.Lang.ResourceManager.GetString("NoticeInfo", System.Globalization.CultureInfo.CurrentUICulture);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取公告失败: {ex}");
                PublicNotice.Text = Properties.Lang.ResourceManager.GetString("NoticeError", System.Globalization.CultureInfo.CurrentUICulture);
            }

            // 计时器
            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 启动时间获取并更新
            TimeSpan Nows = DateTime.Now - UPTime;
            
            // 获取本地化的时间单位
            var dayUnit = Properties.Lang.ResourceManager.GetString("TimeUnitDay", System.Globalization.CultureInfo.CurrentUICulture);
            var hourUnit = Properties.Lang.ResourceManager.GetString("TimeUnitHour", System.Globalization.CultureInfo.CurrentUICulture);
            var minuteUnit = Properties.Lang.ResourceManager.GetString("TimeUnitMinute", System.Globalization.CultureInfo.CurrentUICulture);
            var secondUnit = Properties.Lang.ResourceManager.GetString("TimeUnitSecond", System.Globalization.CultureInfo.CurrentUICulture);
            
            string RunTime_ = $"{Nows.Days} {dayUnit} {Nows.Hours} {hourUnit} {Nows.Minutes} {minuteUnit} {Nows.Seconds} {secondUnit}";
            RunTime.Text = RunTime_;
            CommandManager.InvalidateRequerySuggested();
        }

        private void NoticeTimer_Tick(object sender, EventArgs e)
        {
            if (notices.Count > 1)
            {
                currentNoticeIndex = (currentNoticeIndex + 1) % notices.Count;
                PublicNotice.Opacity = 0;
                string nextNotice = notices[currentNoticeIndex];
                PublicNotice.Text = nextNotice;
                
                // 根据文本长度调整显示时间
                int textLength = nextNotice.Length;
                TimeSpan interval = textLength > 15 ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(3);
                noticeTimer.Interval = interval;
                
                PublicNotice.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)));
            }
        }

        private void Home_Unloaded(object sender, RoutedEventArgs e)
        {
            noticeTimer.Stop();
        }

        private void IPv4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IPv4_Info();
        }

        private void IPv4_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IPv4.Text = "***.***.***.***";
        }

        private void IPv6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IPv6_Info();
        }

        private void IPv6_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IPv6.Text = "****:****:****:****:****:****:****:****";
        }

        private async void IPv4_Info()
        {
            try
            {
                this.IPv4.Text = "Now Loading...";
                HttpResponseMessage IPv4Test = await client.GetAsync("https://myip.ipip.net/");
                IPv4Test.EnsureSuccessStatusCode();
                string IPv4 = await IPv4Test.Content.ReadAsStringAsync();
                this.IPv4.Text = Regex.Replace(IPv4, "[\r\n]", "");
            }
            catch (Exception IPv4Error)
            {
                IPv4.Text = "当前网络可能没有IPV4地址或获取失败";
                Debug.WriteLine(IPv4Error);
            }
        }

        private async void IPv6_Info()
        {
            try
            {
                this.IPv6.Text = "Now Loading...";
                HttpResponseMessage IPv6Test = await client.GetAsync("https://speed.neu6.edu.cn/getIP.php");
                IPv6Test.EnsureSuccessStatusCode();
                string IPv6 = await IPv6Test.Content.ReadAsStringAsync();
                this.IPv6.Text = "当前IPv6地址 : " + IPv6;
            }
            catch (Exception IPv6Error)
            {
                IPv6.Text = "当前网络可能没有IPV6地址或获取失败";
                Debug.WriteLine(IPv6Error);
            }
        }
    }
}
