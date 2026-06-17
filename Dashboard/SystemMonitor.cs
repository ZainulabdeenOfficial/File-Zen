using System.Diagnostics;

namespace FileZenPro.Dashboard;

public class SystemMonitor
{
    private PerformanceCounter cpu =
        new PerformanceCounter("Processor", "% Processor Time", "_Total");

    private PerformanceCounter ram =
        new PerformanceCounter("Memory", "% Committed Bytes In Use");

    public float GetCPU()
    {
        cpu.NextValue();
        Thread.Sleep(500);
        return cpu.NextValue();
    }

    public float GetRAM()
    {
        return ram.NextValue();
    }
}