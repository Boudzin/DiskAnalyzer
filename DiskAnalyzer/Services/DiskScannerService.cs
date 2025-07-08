using DiskAnalyzer.Models;
using System.Linq;

namespace DiskAnalyzer.Services;

public class DiskScannerService
{
    public async Task<List<FileSystemItem>> GetDirectoriesWithSizeAsync(string rootPath)
    {
        return await Task.Run(() => GetDirectoriesWithSize(rootPath));
    }

    // Méthode synchrone qui explore les dossiers et calcule la taille
    private List<FileSystemItem> GetDirectoriesWithSize(string rootPath)
    {
        var directories = new List<FileSystemItem>();
        var dirInfo = new DirectoryInfo(rootPath);

        // Vérifie que le dossier existe
        if (dirInfo.Exists)
        {
            long totalSize = 0;
            try
            {
                // Récupérer les fichiers de manière sécurisée
                var files = dirInfo.GetFiles("*", SearchOption.AllDirectories);

                // Parcourir les fichiers et calculer leur taille
                foreach (var file in files)
                {
                    try
                    {
                        totalSize += file.Length;  // Ajouter la taille du fichier à la taille totale
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignore l'erreur d'accès refusé pour ce fichier et continue avec les autres
                        Console.WriteLine($"Accès refusé au fichier : {file.FullName}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Si le dossier principal est inaccessible, ignorer l'erreur et continuer
                Console.WriteLine($"Accès refusé pour : {rootPath}");
            }

            // Parcours les sous-dossiers et calcule leur taille
            var subDirectories = dirInfo.GetDirectories();
            foreach (var subDir in subDirectories)
            {
                var folderItem = new FileSystemItem
                {
                    Name = subDir.Name,
                    FullPath = subDir.FullName,
                    IsDirectory = true,
                    Size = GetDirectorySize(subDir)
                };
                directories.Add(folderItem);
            }

            // Trie les dossiers par taille (du plus grand au plus petit)
            directories.Sort((d1, d2) => d2.Size.CompareTo(d1.Size));
        }

        return directories;
    }

    // Calcul de la taille d'un dossier, incluant tous ses sous-dossiers et fichiers
    private long GetDirectorySize(DirectoryInfo directory)
    {
        long size = 0;

        // Ajoute la taille des fichiers dans le dossier
        try
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                size += file.Length;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore l'exception si l'accès est refusé au dossier
            Console.WriteLine($"Accès refusé au dossier : {directory.FullName}");
        }
        catch (Exception ex)
        {
            // Gérer d'autres exceptions potentielles (par exemple, des fichiers bloqués)
            Console.WriteLine($"Erreur lors de l'accès à {directory.FullName}: {ex.Message}");
        }

        // Récursion pour les sous-dossiers
        try
        {
            var subDirectories = directory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                size += GetDirectorySize(subDirectory);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore l'exception si l'accès est refusé au sous-dossier
            Console.WriteLine($"Accès refusé au sous-dossier : {directory.FullName}");
        }
        catch (Exception ex)
        {
            // Gérer d'autres exceptions potentielles (par exemple, des dossiers protégés)
            Console.WriteLine($"Erreur lors de l'accès au sous-dossier {directory.FullName}: {ex.Message}");
        }

        return size;
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
