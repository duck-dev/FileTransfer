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
        {
            string path = Path.Combine(folder, file.FileInformation.Name);
            if (File.Exists(path))
                path = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(path)} - Copy{Path.GetExtension(path)}");
            file.FileInformation.CopyTo(path, false);
        }
    }
}