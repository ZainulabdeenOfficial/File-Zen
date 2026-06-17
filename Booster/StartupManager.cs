using System.IO;
using Microsoft.Win32;

namespace FileZenPro.Booster;

public sealed class StartupApp
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class StartupManager
{
    public List<StartupApp> GetStartupAppDetails()
    {
        var apps = new List<StartupApp>();
        AddRegistryApps(apps, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "Current user");
        AddRegistryApps(apps, Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "All users");
        AddStartupFolderApps(apps, Environment.SpecialFolder.Startup, "Startup folder");
        AddStartupFolderApps(apps, Environment.SpecialFolder.CommonStartup, "Common startup folder");
        return apps;
    }

    public List<string> GetStartupApps()
    {
        return GetStartupAppDetails().Select(app => app.Name).ToList();
    }

    private static void AddRegistryApps(List<StartupApp> apps, RegistryKey root, string path, string source)
    {
        try
        {
            using RegistryKey? key = root.OpenSubKey(path);
            if (key is null)
            {
                return;
            }

            foreach (string name in key.GetValueNames())
            {
                apps.Add(new StartupApp
                {
                    Name = name,
                    Source = source,
                    Command = key.GetValue(name)?.ToString() ?? string.Empty
                });
            }
        }
        catch
        {
            // Registry access can vary by machine policy; keep the rest of the app usable.
        }
    }

    private static void AddStartupFolderApps(List<StartupApp> apps, Environment.SpecialFolder folder, string source)
    {
        string path = Environment.GetFolderPath(folder);
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (string file in Directory.EnumerateFiles(path))
        {
            apps.Add(new StartupApp
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Source = source,
                Command = file
            });
        }
    }
}
