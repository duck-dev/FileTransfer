using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls.Notifications;
using FileTransfer.Models;
using FileTransfer.ViewModels;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal static string TemporaryFilesPath { get; } = Path.Combine(FilesParentPath, "TempFiles");

    internal static void DeleteDirectory(string directory)
    {
        if(Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    internal static void SaveFilesToFolder(ICollection<FileObject> files, string location)
    {
        foreach (FileObject file in files)
            SaveFile(file, location, false, false);
        SetRecentDirectory(location, false);
        
        string title = $"{files.Count} files downloaded";
        string message = $"{files.Count} files were downloaded.";
        MainWindowViewModel.ShowNotification(title, message, NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    internal static void SaveFile(FileObject file, string location, bool pathContainsFile, bool setRecentDirectory = true)
    {
        string path;
        string directory;
        if (pathContainsFile)
        {
            path = location;
            directory = Path.GetDirectoryName(location) ?? throw new ArgumentException($"{nameof(location)} refers to a root directory.");
        }
        else
        {
            path = Path.Combine(location, file.FileInformation.Name);
            directory = location;
        }
        
        if (File.Exists(path))
            path = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(path)} - Copy{Path.GetExtension(path)}");
        
        file.FileInformation.CopyTo(path, false);
        if (!setRecentDirectory) 
            return;
        SetRecentDirectory(location, pathContainsFile);
            
        string title = $"{file.FileInformation.Name} downloaded";
        string message = $"{file.FileInformation.Name} was downloaded.";
        MainWindowViewModel.ShowNotification(title, message, NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    private static void SetRecentDirectory(string location, bool pathContainsFile)
    {
        string directory = location;
        if(pathContainsFile)
            directory = Path.GetDirectoryName(location) ?? throw new DirectoryNotFoundException($"{nameof(location)} refers to a root directory.");
        ApplicationVariables.MetaData.RecentDownloadLocation = directory;
    }
}