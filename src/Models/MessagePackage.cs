using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileTransfer.DateFormatterUtility;
using FileTransfer.Events;
using FileTransfer.Extensions;
using FileTransfer.Interfaces;
using FileTransfer.UtilityCollection;
using ReactiveUI;

namespace FileTransfer.Models;

internal sealed class MessagePackage : INotifyPropertyChangedHelper
{
    private enum WaitTime
    {
        OneMinute, 
        EndOfCurrentDay, 
        EndOfNextDay,
        ConstantDate
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? TimeChanged;

    private bool _isRead;
    private string _formattedTimeString = null!;

    internal MessagePackage(DateTime time, User? sender, FileObject[] files, string? textMessage = null)
    {
        this.Files = files;
        this.TextMessage = textMessage;
        this.Time = time;
        this.Sender = sender;

        if (files.Length > 0)
        {
            long overallFilesSize = files.Select(x => x.FileInformation.Length).Sum();
            this.OverallFilesSize = Utilities.DataSizeRepresentation(overallFilesSize);
        }

        this.TimeString = $"{time.ToShortDateString()}    {time.ToLongTimeString()}";
        UpdateTime(WaitTime.OneMinute);

        DownloadZipCommand = ReactiveCommand.Create(DownloadZip);
        DownloadToFolderCommand = ReactiveCommand.Create(DownloadToFolder);
    }

    internal MessagePackage(MessageReceivedEventArgs args) 
        : this(args.Time, args.Sender, args.Files, args.TextMessage)
    { }
    
    internal FileObject[] Files { get; }
    internal string? TextMessage { get; }
    internal DateTime Time { get; }
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
    internal string FormattedTimeString 
    { 
        get => _formattedTimeString;
        private set
        {
            _formattedTimeString = value;
            NotifyPropertyChanged();
        } 
    }

    internal string OverallFilesSize { get; } = "0 B";
    
    internal ReactiveCommand<Unit,Task> DownloadZipCommand { get; }
    internal ReactiveCommand<Unit,Task> DownloadToFolderCommand { get; }

    internal bool HasText => TextMessage is { Length: > 0 }; // TextMessage != null && `TextMessage.Length > 0`
    internal bool HasFiles => Files.Length > 0;

    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private async Task DownloadZip()
    {
        string? directory = ApplicationVariables.RecentDownloadLocation;
        string? result = await Utilities.InvokeOpenFolderDialog("Select the destination folder", directory);
        if (result is null)
            return;
        
        string zipName = $"{Sender?.Nickname}_{Time.Year}-{Time.Month}-{Time.Day}_{Time.Hour}-{Time.Minute}-{Time.Second}";
        Utilities.CreateZip(result, zipName, Files);
        ApplicationVariables.RecentDownloadLocation = result;
    }

    private async Task DownloadToFolder()
    { 
        string? directory = ApplicationVariables.RecentDownloadLocation;
        string? result = await Utilities.InvokeOpenFolderDialog("Select the destination folder", directory);
        if (result is null)
            return;
        
        Utilities.SaveFilesToFolder(Files, result);
    }

    private void ToggleReadStatus() => IsRead = !IsRead;

    private void UpdateTime(WaitTime waitTimeType)
    {
        this.FormattedTimeString = DateFormatter.FormatDate(Time);
        TimeChanged?.Invoke(this, EventArgs.Empty);

        if (waitTimeType == WaitTime.ConstantDate)
            return;
        
        TimeSpan waitTime;
        switch (waitTimeType)
        {
            case WaitTime.OneMinute:
                waitTime = TimeSpan.FromSeconds(60);
                break;
            case WaitTime.EndOfCurrentDay:
            case WaitTime.EndOfNextDay:
                DateTime tomorrow = DateTime.Now.Date.AddDays(1);
                waitTime = tomorrow.Subtract(DateTime.Now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(waitTimeType), waitTimeType, null);
        }
        
        Task.Delay(waitTime).ContinueWith(x => { UpdateTime(waitTimeType.Next()); });
    }
}