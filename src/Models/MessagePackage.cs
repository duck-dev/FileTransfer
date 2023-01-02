using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FileTransfer.DateFormatterUtility;
using FileTransfer.Events;
using FileTransfer.Extensions;
using FileTransfer.Interfaces;
using FileTransfer.ResourcesNamespace;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels;
using FileTransfer.ViewModels.Dialogs;
using FileTransfer.Views;
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

    private bool _isRead;
    private string _formattedTimeString = null!;
    private readonly ReceiveViewModel? _receiveViewModel;

    internal MessagePackage(DateTime time, User? sender, ReceiveViewModel viewModel, FileObject[]? files = null, string? textMessage = null)
    {
        this.Files = files;
        this.TextMessage = textMessage;
        this.Time = time;
        this.Sender = sender;
        _receiveViewModel = viewModel;

        if (files != null)
        {
            long overallFilesSize = files.Select(x => x.FileInformation.Length).Sum();
            this.OverallFilesSize = Utilities.DataSizeRepresentation(overallFilesSize);
        }

        this.TimeString = $"{time.ToShortDateString()}    {time.ToLongTimeString()}";
        UpdateTime(WaitTime.OneMinute);

        DownloadZipCommand = ReactiveCommand.Create(DownloadZip);
        DownloadToFolderCommand = ReactiveCommand.Create(DownloadToFolder);
    }

    internal MessagePackage(MessageReceivedEventArgs args, ReceiveViewModel viewModel) 
        : this(args.Time, args.Sender, viewModel, args.Files, args.TextMessage)
    { }
    
    internal FileObject[]? Files { get; }
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
    
    internal ReactiveCommand<Unit,Unit> DownloadZipCommand { get; }
    internal ReactiveCommand<Unit,Unit> DownloadToFolderCommand { get; }

    public void NotifyPropertyChanged(string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void DownloadZip()
    {
        if (Files is null)
            return;

        async Task ConfirmAction()
        {
            if (MainWindow.Instance is not { } mainWindow)
                return;
            
            var fileDialog = new OpenFolderDialog
            {
                Title = "Select the destination folder"
            };
        
            string? result = await fileDialog.ShowAsync(mainWindow);
            if (result is null)
                return;

            string zipName = $"{Sender?.Nickname}_{Time.Year}-{Time.Month}-{Time.Day}_{Time.Hour}-{Time.Minute}-{Time.Second}";
            Utilities.CreateZip(result, zipName, Files);
        }

        if (_receiveViewModel is null)
            return;
        string dialogTitle = $"Do you really want to download {Files.Length} files in a compressed ZIP-archive?";
        var dialog = new ConfirmationDialogViewModel(_receiveViewModel, dialogTitle, 
            new[] {Resources.MainRed, Resources.MainGrey},
            new[] {Colors.White, Colors.White}, new[] {"Yes, download ZIP!", "Cancel"},
            (Func<Task>) ConfirmAction);
        _receiveViewModel.CurrentDialog = dialog;
    }

    private void DownloadToFolder()
    {
        if (Files is null)
            return;
        
        // TODO: Yet to be implemented
    }

    private void ToggleReadStatus() => IsRead = !IsRead;

    private void UpdateTime(WaitTime waitTimeType)
    {
        this.FormattedTimeString = DateFormatter.FormatDate(Time);

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