using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models;

internal class FileObject
{
    // TODO: Add path to real icons once integrated
    private static readonly Dictionary<string[], string> _knownExtensions = new()
    {
        // General file extensions
        { new [] { "DOC", "DOCX", "DOCM", "DOT", "DOTM", "DOTX", "ODT" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Word
        { new [] { "XLS", "XLSL", "XLSM", "XLSX", "XLT", "XLTM", "XLTX", "XLW", "XLSB", "UOS", "ODS", "CSV" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Excel
        { new [] { "PPT", "PPTX", "ODP", "POT", "POTM", "POTX", "PPA", "PPAM", "PPS", "PPTM", "UOP" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // PowerPoint
        { new [] { "PDF" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // PDF
        { new [] { "TXT", "RTF", "XML", "XAML", "AXAML, JSON" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Text
        { new [] { "ZIP", "Z", "7Z", "DEB", "PKG", "RAR", "TAR.GZ", "RPM", "ARJ", "JAR" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Archives (ZIP etc.)
        { new [] { "PNG", "JPG", "JPEG", "GIF", "BMP", "ICO", "SGV", "RAW", "TIF", "TIFF" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Images
        { new [] { "PSD" }, "" }, // Photoshop
        { new [] { "MP3", "WAV", "PCM", "AIF", "AIFF", "AAC", "OGG", "WMA", "FLAC", "ALAC", "CDA", "MPA", "WPL" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Audio
        { new [] { "MP4", "MOV", "WMV", "AVI", "MKV", "AVCHD", "MPEG2", "FLV", "H264", "M4V", "MPG", "MPEG" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Video
        // Programming languages
        { new [] { "CS" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // C#
        { new [] { "PY" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Python
        { new [] { "JAVA", "CLASS", "JSP", "JAD" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Java
        { new [] { "CPP", "HPP", "CC", "HH", "C++", "H++", "CP", "CXX", "HXX", "C", "H", "II", "IXX", "IPP", "INL", "CPPM" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // C/C++
        { new [] { "PHP" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // PHP
        { new [] { "SH" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Bash
        { new [] { "VB" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Visual Basic
        { new [] { "SWIFT" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Swift
        { new [] { "CGI", "PL" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // Perl
        { new [] { "JS" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // JavaScript
        { new [] { "HTML", "HTM" }, $"{Utilities.AssetsPath}avalonia-logo.ico" }, // HTML
        { new [] { "CSS" }, $"{Utilities.AssetsPath}avalonia-logo.ico" } // CSS
    };

    internal FileObject(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File at path '{path}' not found.");
        FileInformation = new FileInfo(path);
        Size = Utilities.DataSizeRepresentation(FileInformation.Length);
        
        string extension = FileInformation.Extension.ToUpperInvariant().Remove(0, 1); // Get file extension => to upper-case => Remove dot . (.TXT => TXT)
        foreach (KeyValuePair<string[], string> pair in _knownExtensions)
        {
            if (!pair.Key.Contains(extension)) 
                continue;
            FileIcon = Utilities.CreateImage(pair.Value);
            break;
        }
    }
    
    internal FileInfo FileInformation { get; }
    internal Bitmap? FileIcon { get; }
    internal string Size { get; }
}