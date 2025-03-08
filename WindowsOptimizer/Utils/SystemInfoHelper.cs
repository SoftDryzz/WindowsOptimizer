using System;
using System.Diagnostics;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;

namespace WindowsOptimizer.Utils
{
    /// <summary>
    /// Provides system information such as CPU usage, RAM usage, and disk space.
    /// </summary>
    public static class SystemInfoHelper
    {

        private static PerformanceCounter totalCpuCounter;

        /// <summary>
        /// Initializes the CPU performance counter.
        /// </summary>
        public static void InitializeCpuCounter()
        {
            totalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            totalCpuCounter.NextValue();
            Thread.Sleep(500);
        }

        /// <summary>
        /// Gets the total CPU usage across all cores (normalized).
        /// </summary>
        /// <returns>CPU usage percentage.</returns>
        public static double GetCpuUsage()
        {
            if (totalCpuCounter == null)
            {
                InitializeCpuCounter();
            }

            return Math.Round(totalCpuCounter.NextValue(), 1);
        }


        /// <summary>
        /// Retrieves the system's RAM usage and available memory.
        /// </summary>
        /// <returns>A tuple containing used RAM (MB) and available RAM (MB).</returns>
        public static (float used, float available) GetRamUsage()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    float totalRam = Convert.ToSingle(obj["TotalVisibleMemorySize"]) / 1024; // Convert KB to MB
                    float freeRam = Convert.ToSingle(obj["FreePhysicalMemory"]) / 1024;
                    float usedRam = totalRam - freeRam;
                    return (usedRam, freeRam);
                }
            }
            return (0, 0);
        }

        /// <summary>
        /// Retrieves the total and available disk space on drive C:\.
        /// </summary>
        /// <returns>A tuple containing free disk space (GB) and total disk space (GB).</returns>
        public static (float free, float total) GetDiskUsage()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == "C:\\")
                {
                    float totalSpace = drive.TotalSize / (1024 * 1024 * 1024); // Convert bytes to GB
                    float freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024); // Convert bytes to GB
                    return (freeSpace, totalSpace);
                }
            }
            return (0, 0);
        }
    }
}
