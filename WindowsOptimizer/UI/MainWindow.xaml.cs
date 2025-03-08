using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WindowsOptimizer.Utils;

namespace WindowsOptimizer.UI
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _updateTimer;

        /// <summary>
        /// Initializes the main window and sets up automatic system information updates.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            MessageBox.Show(
                "⚠️ IMPORTANT NOTICE ⚠️\n\n" +
                "The CPU usage displayed may differ from Task Manager due to:\n" +
                "- Different update intervals.\n" +
                "- Windows internal optimizations.\n" +
                "- Distribution of CPU load across logical processors.\n\n" +
                "This tool provides an approximate real-time value, but variations may occur.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information
            );

            Task.Run(() =>
            {
                SystemInfoHelper.InitializeCpuCounter();
            });

            Task.Delay(1000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    var updateTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    updateTimer.Tick += UpdateSystemInfo;
                    updateTimer.Start();
                });
            });
        }


        /// <summary>
        /// Handles the event to clean temporary files with animated progress and logs.
        /// </summary>
        private async void CleanTempFiles_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CleanTempFilesButton.IsEnabled = false; 
                StatusText.Text = "Cleaning temporary files...";
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
            });

            var progress = new Progress<int>(value => ProgressBar.Value = value);
            await Task.Run(() => OptimizerHelper.CleanTemporaryFiles(progress));

            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Temporary files cleaned!";
                ProgressBar.Visibility = Visibility.Collapsed;
                CleanTempFilesButton.IsEnabled = true; 
            });
        }


        /// <summary>
        /// Handles the event to free RAM with animated progress and shows freed memory.
        /// </summary>
        private async void FreeRam_Click(object sender, RoutedEventArgs e)
        {

            Dispatcher.Invoke(() =>
            {
                FreeRamButton.IsEnabled = false; 
                StatusText.Text = "Freeing RAM...";
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
            });

            var progress = new Progress<int>(value => ProgressBar.Value = value);
            await Task.Run(() => OptimizerHelper.FreeRAM(progress));

            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "RAM freed!";
                ProgressBar.Visibility = Visibility.Collapsed;
                FreeRamButton.IsEnabled = true; 
            });
        }

        /// <summary>
        /// Updates the UI with the latest system information.
        /// </summary>
        private async void UpdateSystemInfo(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    CpuUsageText.Text = $"CPU Usage: {SystemInfoHelper.GetCpuUsage():0.0} %";
                    var (usedRam, availableRam) = SystemInfoHelper.GetRamUsage();
                    RamUsageText.Text = $"RAM Usage: {usedRam:0.0} MB / {usedRam + availableRam:0.0} MB";
                    var (freeDisk, totalDisk) = SystemInfoHelper.GetDiskUsage();
                    DiskUsageText.Text = $"Disk Space: {freeDisk:0.0} GB / {totalDisk:0.0} GB Free";
                });
            });
        }
    }
}
