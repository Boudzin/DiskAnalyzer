using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DiskAnalyzer.Models;
using DiskAnalyzer.Services;
using Microcharts;
using SkiaSharp;

namespace DiskAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileSystemItem> Items { get; set; } = new();
    public ObservableCollection<FileSystemItem> Folders { get; set; } = new ObservableCollection<FileSystemItem>();
    private readonly DiskScannerService _scanner = new DiskScannerService();
    public string TotalSizeText { get; set; }
    public string FreeSpaceText { get; set; }
    public string UsedSpaceText { get; set; }

    public ICommand ScanCommand { get; }
    public event PropertyChangedEventHandler PropertyChanged;
    public Chart DiskUsageChart { get; set; }

    public MainViewModel()
    {
        ScanCommand = new Command(async () => await ScanAsync());
    }

   
    private async Task ScanAsync()
    {
        var driveInfo = new DriveInfo("C");
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

        var directories = await _scanner.GetDirectoriesWithSizeAsync("C:\\");  // Chemin du disque C
        Folders.Clear();
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
