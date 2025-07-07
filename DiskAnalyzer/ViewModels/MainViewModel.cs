using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DiskAnalyzer.Models;
using DiskAnalyzer.Services;

namespace DiskAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileSystemItem> Items { get; set; } = new();
    private readonly DiskScannerService _scanner = new();
    public string TotalSizeText { get; set; }
    public string FreeSpaceText { get; set; }
    public string UsedSpaceText { get; set; }

    public ICommand ScanCommand { get; }
    public event PropertyChangedEventHandler PropertyChanged;

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

        OnPropertyChanged(nameof(TotalSizeText));
        OnPropertyChanged(nameof(FreeSpaceText));
        OnPropertyChanged(nameof(UsedSpaceText));

        await Shell.Current.DisplayAlert("Info", "Scan lancé", "OK");

        Items.Clear();
        var root = await _scanner.ScanAsync("C:\\"); // Adapter selon OS
        Items.Add(root);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
