using System.IO;
using System.Security.Cryptography;

namespace FileZenPro.Core;

public sealed class DuplicateFile
{
    public string OriginalPath { get; set; } = string.Empty;
    public string DuplicatePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
}

public sealed class DuplicateScanResult
{
    public List<DuplicateFile> Duplicates { get; } = new();
    public int FilesScanned { get; set; }
    public int FailedFiles { get; set; }
    public long DuplicateBytes => Duplicates.Sum(file => file.Size);
}

public class DuplicateDetector
{
    public DuplicateScanResult FindDuplicateDetails(string path, bool recursive = true)
    {
        var result = new DuplicateScanResult();
        if (!Directory.Exists(path))
        {
            return result;
        }

        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (string file in EnumerateFilesSafely(path, option))
        {
            try
            {
                string hash = GetHash(file);
                var info = new FileInfo(file);
                result.FilesScanned++;

                if (hashes.TryGetValue(hash, out string? original))
                {
                    result.Duplicates.Add(new DuplicateFile
                    {
                        OriginalPath = original,
                        DuplicatePath = file,
                        Size = info.Length,
                        Hash = hash
                    });
                }
                else
                {
                    hashes[hash] = file;
                }
            }
            catch
            {
                result.FailedFiles++;
            }
        }

        return result;
    }

    public List<string> FindDuplicates(string path)
    {
        return FindDuplicateDetails(path).Duplicates.Select(file => file.DuplicatePath).ToList();
    }

    private static IEnumerable<string> EnumerateFilesSafely(string path, SearchOption option)
    {
        var pending = new Stack<string>();
        pending.Push(path);

        while (pending.Count > 0)
        {
            string current = pending.Pop();
            string[] files;

            try
            {
                files = Directory.GetFiles(current);
            }
            catch
            {
                continue;
            }

            foreach (string file in files)
            {
                yield return file;
            }

            if (option != SearchOption.AllDirectories)
            {
                continue;
            }

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (string directory in directories)
            {
                pending.Push(directory);
            }
        }
    }

    private static string GetHash(string file)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(file);

        return Convert.ToHexString(sha.ComputeHash(stream));
    }
}
