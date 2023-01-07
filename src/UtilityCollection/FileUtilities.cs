using System;
using System.Collections.Generic;
using System.IO;
using FileTransfer.Models;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal const string TemporaryFilesPath = "TempFiles";
    
    internal static void DeleteElementsInDirectory(string directory) 
        => Directory.Delete(directory, true);

    internal static void SaveFilesToFolder(IEnumerable<FileObject> files, string location)
    {
        foreach (FileObject file in files)
            SaveFile(file, location, false, false);
        SetRecentDirectory(location, false);
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
        if(setRecentDirectory)
            SetRecentDirectory(location, pathContainsFile);
    }

    private static void SetRecentDirectory(string location, bool pathContainsFile)
    {
        string directory = location;
        if(pathContainsFile)
            directory = Path.GetDirectoryName(location) ?? throw new DirectoryNotFoundException($"{nameof(location)} refers to a root directory.");
        ApplicationVariables.RecentDownloadLocation = directory;
    }
}