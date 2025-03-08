using System;
using System.Diagnostics;
using System.Security.Principal;
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

            // Verificar si la app tiene permisos de administrador
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show(
                    "⚠️ This application requires administrator privileges for full functionality.\n\n" +
                    "It will now restart with elevated permissions.",
                    "Administrator Required", MessageBoxButton.OK, MessageBoxImage.Warning
                );

                RestartAsAdministrator();
                Application.Current.Shutdown(); // Cierra la instancia actual
                return;
            }

            // Iniciar el temporizador de actualización
            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            updateTimer.Tick += UpdateSystemInfo;
            updateTimer.Start();
        }

        /// <summary>
        /// Checks if the application is running as an administrator.
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Restarts the application with administrator privileges.
        /// </summary>
        private static void RestartAsAdministrator()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas" // Pide elevación de permisos
            };

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show("Failed to restart with administrator privileges.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
