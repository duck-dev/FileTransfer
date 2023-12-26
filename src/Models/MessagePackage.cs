using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileTransfer.DateFormatterUtility;
using FileTransfer.Enums;
using FileTransfer.Events;
using FileTransfer.Interfaces;
using FileTransfer.UtilityCollection;
using ReactiveUI;

namespace FileTransfer.Models;

internal sealed class MessagePackage : INotifyPropertyChangedHelper, IFormattableTime
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? TimeChanged;

    private bool _isRead;
    private string _formattedTimeString = string.Empty;
    private readonly DateTime _time;

    internal MessagePackage(DateTime time, User? sender, FileObject[] files, string? textMessage = null)
    {
        this.Files = files;
        this.TextMessage = textMessage;
        this.Sender = sender;
        _time = time;

        if (files.Length > 0)
        {
            long overallFilesSize = files.Select(x => x.FileInformation.Length).Sum();
            this.OverallFilesSize = Utilities.DataSizeRepresentation(overallFilesSize);
        }

        this.TimeString = $"{time.ToShortDateString()}    {time.ToLongTimeString()}";
        DateFormatter.UpdateTime(WaitTime.OneMinute, this, _time, () => TimeChanged?.Invoke(this, EventArgs.Empty));

        DownloadZipCommand = ReactiveCommand.Create(DownloadZip);
        DownloadToFolderCommand = ReactiveCommand.Create(DownloadToFolder);
    }

    internal MessagePackage(MessageReceivedEventArgs args) 
        : this(args.Time, args.Sender, args.Files, args.TextMessage)
    { }
    
    public string FormattedTimeString 
    { 
        get => _formattedTimeString;
        set
        {
            _formattedTimeString = value;
            NotifyPropertyChanged();
        } 
    }
    
    internal FileObject[] Files { get; }
    internal string? TextMessage { get; }
    internal User? Sender { get; }

    internal bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            NotifyPropertyChanged();
        }
    }
    
    internal string TimeString { get; }

    internal string OverallFilesSize { get; } = "0 B";
    
    internal ReactiveCommand<Unit,Task> DownloadZipCommand { get; }
    internal ReactiveCommand<Unit,Task> DownloadToFolderCommand { get; }

    internal bool HasText => TextMessage is { Length: > 0 }; // TextMessage != null && `TextMessage.Length > 0`
    internal bool HasFiles => Files.Length > 0;

    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private async Task DownloadZip()
    {
        string? directory = ApplicationVariables.MetaData!.RecentDownloadLocation;
        string? result = await Utilities.InvokeOpenFolderDialog("Select the destination folder", directory);
        if (result is null)
            return;
        
        string zipName = $"{Sender?.Nickname}_{_time.Year}-{_time.Month}-{_time.Day}_{_time.Hour}-{_time.Minute}-{_time.Second}";
        Utilities.CreateZip(result, zipName, Files);
        ApplicationVariables.MetaData.RecentDownloadLocation = result;
    }

    private async Task DownloadToFolder()
    { 
        string? directory = ApplicationVariables.MetaData!.RecentDownloadLocation;
        string? result = await Utilities.InvokeOpenFolderDialog("Select the destination folder", directory);
        if (result is null)
            return;
        
        Utilities.SaveFilesToFolder(Files, result);
    }

    private void ToggleReadStatus() => IsRead = !IsRead;
}