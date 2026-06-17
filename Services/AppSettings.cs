namespace FileZenPro.Services;

public sealed class AppSettings
{
    public bool DarkMode { get; set; } = true;
    public bool IncludeWindowsTemp { get; set; }
    public bool ConfirmBeforeCleaning { get; set; } = true;
    public bool RecursiveFileScan { get; set; } = true;
    public bool DeleteDuplicatesToRecycleBin { get; set; } = true;
    public string OrganizerFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
}
