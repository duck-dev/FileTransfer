using System;
using System.Collections.Generic;
using System.IO;
using FileTransfer.Models;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal const string TemporaryFilesPath = "TempFiles";
    
    internal static void DeleteFilesInDirectory(string directory)
    {
        string[] filePaths = Directory.GetFiles(directory);
        foreach(string path in filePaths)
            File.Delete(path);
    }

    internal static void SaveFilesToFolder(IEnumerable<FileObject> files, string location)
    {
        foreach (FileObject file in files)
            SaveFile(file, location, false);
    }

    internal static void SaveFile(FileObject file, string location, bool pathContainsFile)
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
    } 
}