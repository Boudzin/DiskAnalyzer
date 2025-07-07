namespace DiskAnalyzer.Models;

public class FileSystemItem
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    public List<FileSystemItem> Children { get; set; } = new();

    public string SizeDisplay => $"{Size / (1024.0 * 1024.0):F2} MB";
}
