using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;

namespace WindowsOptimizer.Utils
{
    public static class OptimizerHelper
    {
        /// <summary>
        /// Cleans temporary files with progress updates and logs results.
        /// </summary>
        public static void CleanTemporaryFiles(IProgress<int> progress)
        {
            StringBuilder log = new StringBuilder();
            string tempPath = Environment.GetEnvironmentVariable("TEMP");

            if (string.IsNullOrEmpty(tempPath) || !Directory.Exists(tempPath))
            {
                MessageBox.Show("Temporary folder not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var files = Directory.GetFiles(tempPath);
            var directories = Directory.GetDirectories(tempPath);
            int totalItems = files.Length + directories.Length;
            int processedItems = 0;

            foreach (var file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                    log.AppendLine($"✅ Deleted: {file}");
                }
                catch (UnauthorizedAccessException)
                {
                    log.AppendLine($"❌ Skipped (Access Denied): {file}");
                }
                catch (IOException)
                {
                    log.AppendLine($"❌ Skipped (File in Use): {file}");
                }
                processedItems++;
                progress.Report((processedItems * 100) / totalItems);
            }

            foreach (var dir in directories)
            {
                try
                {
                    Directory.Delete(dir, true);
                    log.AppendLine($"✅ Deleted Folder: {dir}");
                }
                catch (UnauthorizedAccessException)
                {
                    log.AppendLine($"❌ Skipped Folder (Access Denied): {dir}");
                }
                catch (IOException)
                {
                    log.AppendLine($"❌ Skipped Folder (Folder in Use): {dir}");
                }
                processedItems++;
                progress.Report((processedItems * 100) / totalItems);
            }

            progress.Report(100);
            MessageBox.Show(log.ToString(), "Cleanup Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Releases RAM with progress updates and calculates freed memory.
        /// </summary>
        public static void FreeRAM(IProgress<int> progress)
        {
            try
            {
                double beforeRam = GetUsedMemory();

                progress.Report(10);

                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        EmptyWorkingSet(process.Handle);
                    }
                    catch { }
                }

                progress.Report(50);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

                progress.Report(100);

                double afterRam = GetUsedMemory();
                double freedRam = beforeRam - afterRam;

                MessageBox.Show($"RAM has been optimized!\nFreed: {freedRam:0.0} MB",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error freeing RAM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the current system memory usage.
        /// </summary>
        private static double GetUsedMemory()
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    double totalRam = Convert.ToDouble(obj["TotalVisibleMemorySize"]) / 1024;
                    double freeRam = Convert.ToDouble(obj["FreePhysicalMemory"]) / 1024;
                    return totalRam - freeRam; // Used RAM in MB
                }
            }
            return 0;
        }

        [DllImport("psapi.dll")]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
    }
}
