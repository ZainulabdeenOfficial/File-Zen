using System.IO;

namespace FileZenPro.Booster;

public sealed class CleanupSummary
{
    public int FilesDeleted { get; set; }
    public int FilesFound { get; set; }
    public int FailedFiles { get; set; }
    public long BytesFreed { get; set; }
    public List<string> SkippedPaths { get; } = new();

    public long SpaceFreedMB => BytesFreed / 1024 / 1024;
}

public class JunkCleaner
{
    public CleanupSummary Scan(bool includeWindowsTemp = false)
    {
        return ProcessTempFiles(includeWindowsTemp, deleteFiles: false);
    }

    public CleanupSummary CleanDeep(bool includeWindowsTemp = false)
    {
        return ProcessTempFiles(includeWindowsTemp, deleteFiles: true);
    }

    public long CleanDeep()
    {
        return CleanDeep(includeWindowsTemp: false).SpaceFreedMB;
    }

    private static CleanupSummary ProcessTempFiles(bool includeWindowsTemp, bool deleteFiles)
    {
        var paths = new List<string> { Path.GetTempPath() };

        if (includeWindowsTemp)
        {
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));
        }

        var summary = new CleanupSummary();

        foreach (string path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                summary.SkippedPaths.Add(path);
                continue;
            }
            catch (IOException)
            {
                summary.SkippedPaths.Add(path);
                continue;
            }

            foreach (string file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    if (!info.Exists)
                    {
                        continue;
                    }

                    summary.FilesFound++;
                    summary.BytesFreed += info.Length;

                    if (deleteFiles)
                    {
                        info.Delete();
                        summary.FilesDeleted++;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    summary.FailedFiles++;
                }
                catch (IOException)
                {
                    summary.FailedFiles++;
                }
            }
        }

        return summary;
    }
}
