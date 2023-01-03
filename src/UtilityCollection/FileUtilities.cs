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

    internal static void SaveFilesToFolder(IEnumerable<FileObject> files, string folder)
    {
        foreach (FileObject file in files)
            file.FileInformation.CopyTo(Path.Combine(folder, file.FileInformation.Name), false);
    }
}