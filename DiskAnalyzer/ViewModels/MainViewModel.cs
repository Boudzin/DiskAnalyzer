using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DiskAnalyzer.Models;
using DiskAnalyzer.Services;
using Microcharts;
using SkiaSharp;
using CommunityToolkit.Maui.Storage;

namespace DiskAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileSystemItem> Items { get; set; } = new();
    public ObservableCollection<FileSystemItem> Folders { get; set; } = new ObservableCollection<FileSystemItem>();
    private readonly DiskScannerService _scanner = new DiskScannerService();
    private Dictionary<DateTime, string> dicoLastSearch = new Dictionary<DateTime, string>();
    public string TotalSizeText { get; set; }
    public string FreeSpaceText { get; set; }
    public string UsedSpaceText { get; set; }
    public int NumberFolders { get; set; }
    public string NumberFoldersText { get; set; }

    public ICommand ScanCommand { get; }
    public ICommand ScanCommandAnalyzeMainDisk { get; }
    public event PropertyChangedEventHandler PropertyChanged;
    public Chart DiskUsageChart { get; set; }
    public string SelectedFolderPath { get; set; }
    public ICommand PickFolderCommand { get; }

    public MainViewModel()
    {
        PickFolderCommand = new Command(async () => await PickFolderAsync());
        ScanCommand = new Command(async () => await ScanAsync(true));
        ScanCommandAnalyzeMainDisk = new Command(async () => await ScanAsync(false));
    }

    public IEnumerable<RecentSearch> RecentSearches
    {
        get
        {
            return dicoLastSearch
                .OrderByDescending(kv => kv.Key)
                .Select(kv => new RecentSearch { Date = kv.Key, Path = kv.Value });
        }
    }

    private async Task PickFolderAsync()
    {
        var result = await FolderPicker.Default.PickAsync();
        if (result.IsSuccessful)
        {
            SelectedFolderPath = result.Folder.Path;
            OnPropertyChanged(nameof(SelectedFolderPath));
        }
    }

    private async Task ScanAsync(bool selectedPath)
    {
        var driveInfo = new DriveInfo("C");
        #if ANDROID
            driveInfo = new DriveInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
        #endif
        long totalSize = driveInfo.TotalSize;
        long freeSpace = driveInfo.AvailableFreeSpace;
        long usedSpace = totalSize - freeSpace;

        TotalSizeText = $"Total : {totalSize / (1024 * 1024 * 1024)} Go";
        FreeSpaceText = $"Libre : {freeSpace / (1024 * 1024 * 1024)} Go";
        UsedSpaceText = $"Utilisé : {usedSpace / (1024 * 1024 * 1024)} Go";

        OnPropertyChanged(nameof(DiskUsageChart));
        OnPropertyChanged(nameof(TotalSizeText));
        OnPropertyChanged(nameof(FreeSpaceText));
        OnPropertyChanged(nameof(UsedSpaceText));
        #if WINDOWS
            if (!selectedPath)
            {
                SelectedFolderPath = "C:\\";
            }
        #elif MACCATALYST
                    if (!selectedPath)
                    {
                        SelectedFolderPath = "/"; // Chemin racine pour macOS
                    }
        #elif ANDROID
                    if (!selectedPath)
                    {
                        SelectedFolderPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath; // Chemin du stockage externe
                    }
        #endif
        var directories = await _scanner.GetDirectoriesWithSizeAsync(SelectedFolderPath);  // Chemin du disque C ou chemin sélectionné
        dicoLastSearch.Add(DateTime.Now, SelectedFolderPath);
        OnPropertyChanged(nameof(RecentSearches));
        SelectedFolderPath = null; // Réinitialise le chemin sélectionné après la récupération
        Folders.Clear();
        NumberFolders = directories.Count;
        NumberFoldersText = $"{NumberFolders} dossiers";
        OnPropertyChanged(nameof(NumberFoldersText));
        // Ajoute chaque dossier à la collection
        foreach (var folder in directories)
        {
            Folders.Add(folder);
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
