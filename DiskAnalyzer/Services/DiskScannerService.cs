using DiskAnalyzer.Models;

namespace DiskAnalyzer.Services;

public class DiskScannerService
{
    public async Task<FileSystemItem> ScanAsync(string path)
    {
        return await Task.Run(() => Scan(path));
    }

    private FileSystemItem Scan(string path)
    {
        var item = new FileSystemItem
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            IsDirectory = Directory.Exists(path)
        };

        if (item.IsDirectory)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    var child = Scan(dir);
                    item.Size += child.Size;
                    item.Children.Add(child);
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    var info = new FileInfo(file);
                    item.Size += info.Length;
                    item.Children.Add(new FileSystemItem
                    {
                        Name = info.Name,
                        FullPath = info.FullName,
                        Size = info.Length,
                        IsDirectory = false
                    });
                }
            }
            catch { }
        }
        else
        {
            var info = new FileInfo(path);
            item.Size = info.Length;
        }

        return item;
    }
}
