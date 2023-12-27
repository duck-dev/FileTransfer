using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls.Notifications;
using FileTransfer.Models;
using FileTransfer.ResourcesNamespace;
using FileTransfer.ViewModels;
using FileTransfer.ViewModels.Dialogs;

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

        try
        {
            file.FileInformation.CopyTo(path, true);
        }
        catch (IOException e)
        {
            if (IsDiskFull(e))
                ShowDiskFullDialog();
        }
        
        if (setRecentDirectory) 
            SetRecentDirectory(location, pathContainsFile);
            
        string title = $"{file.FileInformation.Name} downloaded";
        string message = $"{file.FileInformation.Name} was downloaded.";
        MainWindowViewModel.ShowNotification(title, message, NotificationType.Success, TimeSpan.FromSeconds(3));
    }
    
    internal static bool IsDiskFull(Exception ex)
    {
        const int hrErrorHandleDiskFull = unchecked((int)0x80070027);
        const int hrErrorDiskFull = unchecked((int)0x80070070);

        return ex.HResult == hrErrorHandleDiskFull || ex.HResult == hrErrorDiskFull;
    }

    internal static void ShowDiskFullDialog()
    {
        if (ReceiveViewModel.Instance is not { } receiveViewModel)
            return;
        const string dialogTitle = "There is not enough space on the hard drive. The file(s) could not be downloaded.";
        receiveViewModel.CurrentDialog = new InformationDialogViewModel(receiveViewModel, dialogTitle, new [] { Resources.AppPurpleBrush}, 
            new [] { Resources.WhiteBrush }, new [] { "Ok!" });
    }

    private static void SetRecentDirectory(string location, bool pathContainsFile)
    {
        string directory = location;
        if(pathContainsFile)
            directory = Path.GetDirectoryName(location) ?? throw new DirectoryNotFoundException($"{nameof(location)} refers to a root directory.");
        ApplicationVariables.MetaData!.RecentDownloadLocation = directory;
    }
}