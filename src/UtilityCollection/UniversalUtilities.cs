using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using FileTransfer.Models;
using FileTransfer.ViewModels;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal const string AssetsPath = "avares://FileTransfer/Assets/";
    
    /// <summary>
    /// The parent path of all settings- and data-files
    /// </summary>
    public static string FilesParentPath
    {
        get
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string directory = Path.Combine(appData, "FileTransfer");
#if DEBUG
            directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? throw new DirectoryNotFoundException("Directory name of the currently executing assembly is null.");
#endif
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }
    }
    
    /// <summary>
    /// Log a message to the debug output (for debugging purposes).
    /// </summary>
    /// <param name="message">The message to be logged as a string.</param>
    internal static void Log(string? message)
    {
        Console.WriteLine(message);
        System.Diagnostics.Trace.WriteLine(message);
    }
    
    /// <summary>
    /// Retrieves a resource from a specified <see cref="IResourceNode"/> and tries to cast it to the specified type.
    /// </summary>
    /// <param name="element">The element to retrieve the resource from.</param>
    /// <param name="resourceName">The name of the resource you want to retrieve (key).</param>
    /// <typeparam name="T">The actual type of the resource you want to retrieve.</typeparam>
    /// <returns>The resource as it's actual type.</returns>
    internal static T? GetResource<T>(IResourceNode element, string resourceName) where T : class
    {
        element.TryGetResource(resourceName, out object? resource);
        return resource as T;
    }

    /// <summary>
    /// Retrieves a resource from a specified <see cref="Style"/>, which is in turns contained
    /// in the <see cref="Styles"/> of an <see cref="IStyleHost"/>, and tries to cast it to the specified type.
    /// </summary>
    /// <param name="element">The element to retrieve the resource from.</param>
    /// <param name="resourceName">The name of the resource you want to retrieve (key).</param>
    /// <param name="styleIndex">The index of the <see cref="Style"/> inside the <see cref="Styles"/> collection
    /// of the <paramref name="element"/>.</param>
    /// <typeparam name="TResource">The actual type of the resource you want to retrieve.</typeparam>
    /// <typeparam name="TElement">The type of the element to retrieve the resource from.
    /// This type must implement <see cref="IResourceNode"/>.</typeparam>
    /// <returns>The resource as it's actual type.</returns>
    internal static TResource? GetResourceFromStyle<TResource, TElement>(TElement? element, string resourceName, int styleIndex)
        where TResource : class
        where TElement : IStyleHost, IResourceNode
    {
        var styleInclude = element?.Styles[styleIndex] as StyleInclude;
        return (styleInclude?.Loaded is Style style ? GetResource<TResource>(style, resourceName) : null);
    }
    
    /// <summary>
    /// Creates a <see cref="Bitmap"/> image, based on an image-file whose path is specified as a passed argument.
    /// </summary>
    /// <param name="path">The path of the image-file.</param>
    /// <returns>The <see cref="Bitmap"/> image or null if the operation was successful due to a wrong path for example.</returns>
    internal static Bitmap? CreateImage(string path)
    {
        var uri = new Uri(path);
        Stream? asset = null;
        try
        {
            IAssetLoader? assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            asset = assets?.Open(uri);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return asset is null ? null : new Bitmap(asset);
    }

    /// <summary>
    /// Formats a given data size in bytes into a matching representation of Bytes, Kilobytes, Megabytes or Gigabytes as a <see cref="string"/>.
    /// </summary>
    /// <param name="bytes">The data size in bytes.</param>
    /// <returns>The formatted representation of the data size.</returns>
    internal static string DataSizeRepresentation(long bytes)
    {
        return bytes switch
        {
            < 1000 => $"{bytes:F1} B",
            < 1000000 => $"{(bytes / 1000f):F1} KB",
            < 1000000000 => $"{(bytes / 1000000f):F1} MB",
            _ => $"{(bytes / 1000000000f):F1} GB"
        };
    }

    internal static void CreateZip(string folder, string zipName, ICollection<FileObject> files)
    {
        string zipPath = Path.Combine(folder, $"{zipName}.zip");
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (FileObject file in files)
        {
            string filePath = file.FileInformation.FullName;
            string fileName = file.FileInformation.Name;
            archive.CreateEntryFromFile(filePath, fileName);
        }

        const string title = "ZIP downloaded";
        string message = $"A ZIP file with {files.Count} files was downloaded.";
        MainWindowViewModel.ShowNotification(title, message, NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    internal static void ShowFlyout(Control button, IEnumerable<MenuItem> items, FlyoutPlacementMode placement)
    {
        MenuFlyout flyout = new MenuFlyout
        {
            Items = items,
            Placement = placement,
        };
        FlyoutBase.SetAttachedFlyout(button, flyout);
        FlyoutBase.ShowAttachedFlyout(button);
    }

    internal static int CompareStringsAlphabetically(string? a, string? b) => string.Compare(a, b, StringComparison.Ordinal);
}