using System.IO;
using FileZenPro.Core;

namespace FileZenPro.Automation;

public class FolderWatcher
{
    private FileOrganizer organizer = new FileOrganizer();

    public void Start(string path)
    {
        var watcher = new FileSystemWatcher(path);
        watcher.Created += (s, e) =>
        {
            organizer.Organize(path);
        };
        watcher.EnableRaisingEvents = true;
    }
}