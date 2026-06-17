using System.Diagnostics;
using FileZenPro.Booster;

namespace FileZen.Booster;

public sealed class TurboResult
{
    public CleanupSummary Cleanup { get; set; } = new();
    public int ProcessesAdjusted { get; set; }
}

public class TurboEngine
{
    public TurboResult Activate(bool includeWindowsTemp = false)
    {
        JunkCleaner cleaner = new JunkCleaner();
        SystemBooster booster = new SystemBooster();

        var result = new TurboResult
        {
            Cleanup = cleaner.CleanDeep(includeWindowsTemp),
            ProcessesAdjusted = booster.Boost()
        };

        Process currentProcess = Process.GetCurrentProcess();
        try
        {
            currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        catch
        {
            // Some environments block priority changes; Turbo still cleaned junk safely.
        }

        return result;
    }
}
