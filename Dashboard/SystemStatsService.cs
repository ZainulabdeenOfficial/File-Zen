using FileZen.Models;
using FileZenPro.Dashboard;

namespace FileZen.Dashboard
{
    public class SystemStatsService
    {
        private readonly SystemMonitor monitor = new SystemMonitor();

        public SystemStats GetStats()
        {
            return new SystemStats
            {
                CPUUsage = monitor.GetCPU(),
                RAMUsage = monitor.GetRAM(),
                DiskUsage = 0
            };
        }
    }
}

