using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FileZen.Booster;
using FileZen.Core;
using FileZenPro.Booster;
using FileZenPro.Core;
using FileZenPro.Dashboard;
using FileZenPro.Services;
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace FileZenPro;

public partial class MainWindow : MetroWindow
{
    private readonly SystemMonitor monitor = new();
    private readonly StartupManager startupManager = new();
    private readonly JunkCleaner junkCleaner = new();
    private readonly SystemBooster systemBooster = new();
    private readonly AppSettingsService settingsService = new();
    private readonly DispatcherTimer monitorTimer = new();
    private AppSettings settings;
    private bool isReadingSystemStats;
    private readonly ObservableCollection<string> activityItems = new();

    public ChartValues<float> CpuValues { get; } = new();
    public ChartValues<float> RamValues { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        settings = settingsService.Load();
        ApplySettingsToUi();
        ActivityListBox.ItemsSource = activityItems;
        SetupCharts();
        RefreshStartupApps();
        ShowPanel(DashboardPanel);
        StartMonitoring();
    }

    private void SetupCharts()
    {
        CpuChart.Series = new SeriesCollection
        {
            new LineSeries { Values = CpuValues, Title = "CPU", PointGeometry = null }
        };

        RamChart.Series = new SeriesCollection
        {
            new LineSeries { Values = RamValues, Title = "RAM", PointGeometry = null }
        };
    }

    private void StartMonitoring()
    {
        monitorTimer.Interval = TimeSpan.FromSeconds(2);
        monitorTimer.Tick += async (_, _) => await UpdateSystemStatsAsync();
        monitorTimer.Start();
        _ = UpdateSystemStatsAsync();
    }

    private async Task UpdateSystemStatsAsync()
    {
        if (isReadingSystemStats)
        {
            return;
        }

        isReadingSystemStats = true;
        try
        {
            var stats = await Task.Run(() => new
            {
                Cpu = monitor.GetCPU(),
                Ram = monitor.GetRAM()
            });

            AddChartValue(CpuValues, stats.Cpu);
            AddChartValue(RamValues, stats.Ram);

            CpuText.Text = $"{stats.Cpu:0}%";
            RamText.Text = $"{stats.Ram:0}%";
        }
        catch (Exception ex)
        {
            AddActivity($"System monitor unavailable: {ex.Message}");
        }
        finally
        {
            isReadingSystemStats = false;
        }
    }

    private static void AddChartValue(ChartValues<float> values, float value)
    {
        values.Add(value);
        if (values.Count > 60)
        {
            values.RemoveAt(0);
        }
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(DashboardPanel);
    }

    private void Organizer_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(OrganizerPanel);
    }

    private void Duplicates_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(DuplicatesPanel);
    }

    private void Booster_Click(object sender, RoutedEventArgs e)
    {
        RefreshStartupApps();
        ShowPanel(BoosterPanel);
    }

    private void Cleaner_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(CleanerPanel);
    }

    private void TurboTab_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(TurboPanel);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(SettingsPanel);
    }

    private void ShowPanel(Grid activePanel)
    {
        Grid[] panels =
        {
            DashboardPanel,
            OrganizerPanel,
            DuplicatesPanel,
            BoosterPanel,
            CleanerPanel,
            TurboPanel,
            SettingsPanel
        };

        foreach (Grid panel in panels)
        {
            panel.Visibility = panel == activePanel ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose a folder to organize",
            InitialDirectory = Directory.Exists(OrganizerPathTextBox.Text)
                ? OrganizerPathTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) == true)
        {
            OrganizerPathTextBox.Text = dialog.FolderName;
            DuplicatePathTextBox.Text = dialog.FolderName;
            settings.OrganizerFolder = dialog.FolderName;
            settingsService.Save(settings);
            ScanFolder();
        }
    }

    private void ScanFolder_Click(object sender, RoutedEventArgs e)
    {
        ScanFolder();
    }

    private void ScanFolder()
    {
        string path = OrganizerPathTextBox.Text.Trim();
        var files = new FileScanner().Scan(path);
        FilesGrid.ItemsSource = files;
        AddActivity(Directory.Exists(path)
            ? $"Found {files.Count} files in {path}."
            : "Choose an existing folder first.");
    }

    private void OrganizeFolder_Click(object sender, RoutedEventArgs e)
    {
        string path = OrganizerPathTextBox.Text.Trim();
        if (!Directory.Exists(path))
        {
            MessageBox.Show(this, "Choose an existing folder before organizing.", "FileZen");
            return;
        }

        try
        {
            new FileOrganizer().Organize(path);
            ScanFolder();
            AddActivity("Folder organized by file type.");
            MessageBox.Show(this, "Files were organized into category folders.", "FileZen");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not organize this folder: {ex.Message}", "FileZen");
        }
    }

    private void RefreshStartup_Click(object sender, RoutedEventArgs e)
    {
        RefreshStartupApps();
    }

    private void RefreshStartupApps()
    {
        var apps = startupManager.GetStartupAppDetails();
        StartupGrid.ItemsSource = apps;
        StartupCountText.Text = apps.Count.ToString();
        AddActivity($"Loaded {apps.Count} startup entries.");
    }

    private void Boost_Click(object sender, RoutedEventArgs e)
    {
        int adjusted = systemBooster.Boost();
        AddActivity($"Boost complete. Adjusted {adjusted} background processes.");
        MessageBox.Show(this, $"Boost complete. Adjusted {adjusted} background processes.", "FileZen");
    }

    private void UseOrganizerFolderForDuplicates_Click(object sender, RoutedEventArgs e)
    {
        DuplicatePathTextBox.Text = OrganizerPathTextBox.Text;
        AddActivity("Duplicate scan folder set from organizer.");
    }

    private void ScanDuplicates_Click(object sender, RoutedEventArgs e)
    {
        string path = DuplicatePathTextBox.Text.Trim();
        if (!Directory.Exists(path))
        {
            MessageBox.Show(this, "Choose an existing folder before scanning duplicates.", "FileZen");
            return;
        }

        DuplicateScanResult result = new DuplicateDetector().FindDuplicateDetails(path, settings.RecursiveFileScan);
        DuplicateGrid.ItemsSource = result.Duplicates;
        DuplicateResultText.Text =
            $"Scanned {result.FilesScanned} files. Found {result.Duplicates.Count} duplicates using {FormatBytes(result.DuplicateBytes)}. Failed files: {result.FailedFiles}.";
        AddActivity($"Duplicate scan found {result.Duplicates.Count} files.");
    }

    private void DeleteSelectedDuplicates_Click(object sender, RoutedEventArgs e)
    {
        var selected = DuplicateGrid.SelectedItems.Cast<DuplicateFile>().ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, "Select duplicate files to delete first.", "FileZen");
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            this,
            $"Delete {selected.Count} selected duplicate files?",
            "FileZen Duplicates",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        int deleted = 0;
        int failed = 0;
        foreach (DuplicateFile duplicate in selected)
        {
            try
            {
                if (settings.DeleteDuplicatesToRecycleBin)
                {
                    FileSystem.DeleteFile(
                        duplicate.DuplicatePath,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                }
                else
                {
                    File.Delete(duplicate.DuplicatePath);
                }

                deleted++;
            }
            catch
            {
                failed++;
            }
        }

        AddActivity($"Deleted {deleted} duplicates. Failed: {failed}.");
        ScanDuplicates_Click(sender, e);
    }

    private void ScanJunk_Click(object sender, RoutedEventArgs e)
    {
        CleanupSummary summary = junkCleaner.Scan(settings.IncludeWindowsTemp);
        CleanerResultText.Text = FormatCleanupSummary("Scan complete", summary, deleted: false);
        AddActivity($"Junk scan found {summary.FilesFound} files.");
    }

    private void CleanJunk_Click(object sender, RoutedEventArgs e)
    {
        if (settings.ConfirmBeforeCleaning)
        {
            MessageBoxResult confirm = MessageBox.Show(
                this,
                "Delete temporary files that are safe for the current user account?",
                "FileZen Cleaner",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }
        }

        CleanupSummary summary = junkCleaner.CleanDeep(settings.IncludeWindowsTemp);
        CleanerResultText.Text = FormatCleanupSummary("Cleanup complete", summary, deleted: true);
        AddActivity($"Cleaned {summary.FilesDeleted} files and freed {FormatBytes(summary.BytesFreed)}.");
    }

    private void Turbo_Click(object sender, RoutedEventArgs e)
    {
        TurboResult result = new TurboEngine().Activate(settings.IncludeWindowsTemp);
        TurboResultText.Text =
            $"Turbo complete. Freed {FormatBytes(result.Cleanup.BytesFreed)} and adjusted {result.ProcessesAdjusted} background processes.";
        AddActivity("Turbo mode completed.");
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        settings.DarkMode = DarkModeCheckBox.IsChecked == true;
        settings.ConfirmBeforeCleaning = ConfirmCleanCheckBox.IsChecked == true;
        settings.RecursiveFileScan = RecursiveScanCheckBox.IsChecked == true;
        settings.DeleteDuplicatesToRecycleBin = RecycleDuplicatesCheckBox.IsChecked == true;
        settings.IncludeWindowsTemp = IncludeWindowsTempCheckBox.IsChecked == true;
        settings.OrganizerFolder = OrganizerPathTextBox.Text.Trim();

        settingsService.Save(settings);
        ApplyTheme();
        AddActivity("Settings saved.");
        MessageBox.Show(this, "Settings saved.", "FileZen");
    }

    private void ApplySettingsToUi()
    {
        OrganizerPathTextBox.Text = settings.OrganizerFolder;
        DuplicatePathTextBox.Text = settings.OrganizerFolder;
        DarkModeCheckBox.IsChecked = settings.DarkMode;
        ConfirmCleanCheckBox.IsChecked = settings.ConfirmBeforeCleaning;
        RecursiveScanCheckBox.IsChecked = settings.RecursiveFileScan;
        RecycleDuplicatesCheckBox.IsChecked = settings.DeleteDuplicatesToRecycleBin;
        IncludeWindowsTempCheckBox.IsChecked = settings.IncludeWindowsTemp;
        ApplyTheme();
    }

    private void AddActivity(string message)
    {
        string item = $"{DateTime.Now:HH:mm}  {message}";
        activityItems.Insert(0, item);
        while (activityItems.Count > 8)
        {
            activityItems.RemoveAt(activityItems.Count - 1);
        }

        StatusText.Text = message;
    }

    private void ApplyTheme()
    {
        bool darkMode = DarkModeCheckBox.IsChecked == true;
        Background = (Brush)new BrushConverter().ConvertFromString(darkMode ? "#0F0F0F" : "#F3F4F6")!;
    }

    private static string FormatCleanupSummary(string title, CleanupSummary summary, bool deleted)
    {
        string action = deleted ? "Deleted" : "Found";
        string skipped = summary.SkippedPaths.Count == 0
            ? string.Empty
            : $" Skipped: {string.Join(", ", summary.SkippedPaths)}.";

        return $"{title}. {action} {summary.FilesDeleted} of {summary.FilesFound} files. " +
               $"Recoverable space: {FormatBytes(summary.BytesFreed)}. Failed files: {summary.FailedFiles}.{skipped}";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024L * 1024L * 1024L)
        {
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GB";
        }

        if (bytes >= 1024L * 1024L)
        {
            return $"{bytes / 1024d / 1024d:0.##} MB";
        }

        if (bytes >= 1024L)
        {
            return $"{bytes / 1024d:0.##} KB";
        }

        return $"{bytes} bytes";
    }
}
