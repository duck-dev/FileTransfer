using System.IO;

namespace FileTransfer.UtilityCollection;

public static partial class Utilities
{
    public const string TemporaryFilesPath = "TempFiles";
    
    public static void DeleteFilesInDirectory(string directory)
    {
        string[] filePaths = Directory.GetFiles(directory);
        foreach(string path in filePaths)
            File.Delete(path);
    }
}