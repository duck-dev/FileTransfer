namespace FileTransfer.Models;

internal static class ApplicationVariables
{
    // TODO: Set to default download location set in the settings (default: "Downloads" folder) by default
    internal static string? RecentDownloadLocation { get; set; } // = DEFAULT_LOCATION
    
    // TODO: Set to default upload location set in the settings (default: YET TO BE DEFINED) by default
    internal static string? RecentUploadLocation { get; set; } // = DEFAULT_LOCATION
}