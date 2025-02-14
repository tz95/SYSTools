using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;
using System.Linq;
using System.Diagnostics;

namespace SYSTools.Pages
{
    /// <summary>
    /// HardwareMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareMonitor : Page
    {
        private readonly Computer computer;
        private readonly DispatcherTimer timer;
        private const int RefreshInterval = 2; // 刷新间隔（秒）
        private readonly Dictionary<string, TreeViewItem> hardwareItems = new Dictionary<string, TreeViewItem>();
        private readonly Dictionary<string, TreeViewItem> sensorItems = new Dictionary<string, TreeViewItem>();
        private bool isInitialized = false;

        public HardwareMonitor()
        {
            InitializeComponent();
            
            // 初始化硬件监控
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true,
                IsBatteryEnabled = true
            };
            
            try
            {
                computer.Open();
                computer.Accept(new UpdateVisitor()); // 立即更新一次以确保初始化所有硬件
                
                // 输出调试信息
                foreach (var hardware in computer.Hardware)
                {
                    Debug.WriteLine($"Found hardware: {hardware.HardwareType} - {hardware.Name}");
                    if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        Debug.WriteLine($"GPU details: {hardware.Name}");
                        foreach (var sensor in hardware.Sensors)
                        {
                            Debug.WriteLine($"  Sensor: {sensor.Name} - {sensor.SensorType}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing hardware monitoring: {ex}");
                MessageBox.Show($"初始化硬件监控时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // 初始化定时器
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(RefreshInterval)
            };
            timer.Tick += Timer_Tick;

            // 注册页面事件
            Loaded += HardwareMonitor_Loaded;
            Unloaded += HardwareMonitor_Unloaded;
            IsVisibleChanged += HardwareMonitor_IsVisibleChanged;
        }

        private void HardwareMonitor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
            {
                InitializeTreeView();
                isInitialized = true;
            }
            StartMonitoring();
        }

        private void HardwareMonitor_Unloaded(object sender, RoutedEventArgs e)
        {
            StopMonitoring();
        }

        private void HardwareMonitor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool isVisible)
            {
                if (isVisible)
                {
                    if (!isInitialized)
                    {
                        InitializeTreeView();
                        isInitialized = true;
                    }
                    StartMonitoring();
                }
                else
                {
                    StopMonitoring();
                }
            }
        }

        private void StartMonitoring()
        {
            try
            {
                UpdateSensorValues(); // 立即更新一次
                timer.Start();
                Debug.WriteLine("Hardware monitoring started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting monitoring: {ex}");
            }
        }

        private void StopMonitoring()
        {
            timer.Stop();
            Debug.WriteLine("Hardware monitoring stopped");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateSensorValues();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating sensor values: {ex}");
            }
        }

        private void InitializeTreeView()
        {
            try
            {
                TestView.Items.Clear();
                hardwareItems.Clear();
                sensorItems.Clear();

                computer.Accept(new UpdateVisitor()); // 确保数据是最新的

                foreach (IHardware hardware in computer.Hardware)
                {
                    var hardwareItem = new TreeViewItem
                    {
                        Header = $"{hardware.Name} ({hardware.HardwareType})",
                        IsExpanded = true
                    };
                    hardwareItems[hardware.Identifier.ToString()] = hardwareItem;

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        var sensorItem = new TreeViewItem();
                        string sensorKey = $"{hardware.Identifier}_{sensor.Identifier}";
                        sensorItems[sensorKey] = sensorItem;
                        UpdateSensorItem(sensorItem, sensor);
                        hardwareItem.Items.Add(sensorItem);
                    }

                    if (hardwareItem.Items.Count > 0)
                    {
                        TestView.Items.Add(hardwareItem);
                    }
                }
                Debug.WriteLine("TreeView initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing TreeView: {ex}");
            }
        }

        private void UpdateSensorValues()
        {
            if (!isInitialized || TestView.Items.Count == 0)
            {
                InitializeTreeView();
                isInitialized = true;
                return;
            }

            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {
                foreach (ISensor sensor in hardware.Sensors)
                {
                    string sensorKey = $"{hardware.Identifier}_{sensor.Identifier}";
                    if (sensorItems.TryGetValue(sensorKey, out TreeViewItem sensorItem))
                    {
                        UpdateSensorItem(sensorItem, sensor);
                    }
                }
            }
        }

        private void UpdateSensorItem(TreeViewItem item, ISensor sensor)
        {
            if (sensor.Value.HasValue)
            {
                string value = FormatSensorValue(sensor);
                item.Header = $"{sensor.Name}: {value}";
                item.Visibility = Visibility.Visible;
            }
            else
            {
                item.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatSensorValue(ISensor sensor)
        {
            if (!sensor.Value.HasValue)
                return "N/A";

            return sensor.SensorType switch
            {
                SensorType.Temperature => $"{sensor.Value:F1} °C",
                SensorType.Load => $"{sensor.Value:F1} %",
                SensorType.Fan => $"{sensor.Value:F0} RPM",
                SensorType.Flow => $"{sensor.Value:F0} L/h",
                SensorType.Control => $"{sensor.Value:F1} %",
                SensorType.Level => $"{sensor.Value:F1} %",
                SensorType.Power => $"{sensor.Value:F1} W",
                SensorType.Data => $"{sensor.Value:F1} GB",
                SensorType.SmallData => $"{sensor.Value:F1} MB",
                SensorType.Voltage => $"{sensor.Value:F3} V",
                SensorType.Clock => $"{sensor.Value:F1} MHz",
                SensorType.Factor => $"{sensor.Value:F3}",
                _ => $"{sensor.Value:F1}"
            };
        }
        private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetTreeViewItemsExpandState(false);
        }

        private void ExpandAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetTreeViewItemsExpandState(true);
        }

        private void SetTreeViewItemsExpandState(bool isExpanded)
        {
            foreach (TreeViewItem item in TestView.Items)
            {
                ExpandCollapseTreeViewItem(item, isExpanded);
            }
        }

        private void ExpandCollapseTreeViewItem(TreeViewItem item, bool isExpanded)
        {
            item.IsExpanded = isExpanded;
            foreach (TreeViewItem childItem in item.Items.OfType<TreeViewItem>())
            {
                ExpandCollapseTreeViewItem(childItem, isExpanded);
            }
        }
    }

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            try
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware)
                {
                    subHardware.Accept(this);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating hardware {hardware.Name}: {ex}");
            }
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }
}
