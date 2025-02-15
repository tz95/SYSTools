using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WUApiLib;

namespace SYSTools.WindowsToolsPages
{
    /// <summary>
    /// WindowsUpdateHistory.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsUpdateHistory : Page
    {
        private ObservableCollection<UpdateItem> UpdateItems { get; set; }

        public WindowsUpdateHistory()
        {
            InitializeComponent();
            UpdateItems = new ObservableCollection<UpdateItem>();
            UpdateHistoryGrid.ItemsSource = UpdateItems;
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadButton.IsEnabled = false;
            try
            {
                await LoadUpdateHistory();
            }
            finally
            {
                LoadButton.IsEnabled = true;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateItems.Clear();
        }

        private async Task LoadUpdateHistory()
        {
            try
            {
                LoadingRing.IsActive = true;
                UpdateItems.Clear();

                await Task.Run(() =>
                {
                    var updateSession = new UpdateSession();
                    var updateSearcher = updateSession.CreateUpdateSearcher();
                    var count = updateSearcher.GetTotalHistoryCount();
                    var history = updateSearcher.QueryHistory(0, count);

                    for (int i = 0; i < count; i++)
                    {
                        var update = history[i];
                        Dispatcher.Invoke(() =>
                        {
                            UpdateItems.Add(new UpdateItem
                            {
                                Title = update.Title,
                                Date = update.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                                Status = GetUpdateStatus(update.ResultCode)
                            });
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading update history: {ex}");
                MessageBox.Show(
                    Properties.Lang.ResourceManager.GetString("UpdateHistoryError", System.Globalization.CultureInfo.CurrentUICulture),
                    Properties.Lang.ResourceManager.GetString("ErrorTitle", System.Globalization.CultureInfo.CurrentUICulture),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private string GetUpdateStatus(OperationResultCode resultCode)
        {
            return resultCode switch
            {
                OperationResultCode.orcSucceeded => Properties.Lang.ResourceManager.GetString("UpdateStatusSuccess", System.Globalization.CultureInfo.CurrentUICulture),
                OperationResultCode.orcFailed => Properties.Lang.ResourceManager.GetString("UpdateStatusFailed", System.Globalization.CultureInfo.CurrentUICulture),
                _ => Properties.Lang.ResourceManager.GetString("UpdateStatusUnknown", System.Globalization.CultureInfo.CurrentUICulture)
            };
        }
    }

    public class UpdateItem
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }
}
