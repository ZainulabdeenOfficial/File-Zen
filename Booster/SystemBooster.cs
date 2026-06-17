using System.Diagnostics;

namespace FileZenPro.Booster;

public class SystemBooster
{
    public int Boost()
    {
        int adjusted = 0;
        string[] backgroundProcessHints = { "OneDrive", "Teams", "Discord", "Steam", "EpicGamesLauncher" };

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (backgroundProcessHints.Any(name =>
                        process.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    process.PriorityClass = ProcessPriorityClass.BelowNormal;
                    adjusted++;
                }
            }
            catch
            {
                // Access to many system processes is expected to be denied for normal users.
            }
        }

        return adjusted;
    }
}
