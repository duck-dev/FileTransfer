using System;
using System.Collections.Generic;
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

    internal MessagePackage(DateTime time, User? sender, ReceiveViewModel viewModel, FileObject[] files, string? textMessage = null)
    {
        this.Files = files;
        this.TextMessage = textMessage;
        this.Time = time;
        this.Sender = sender;
        _receiveViewModel = viewModel;

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

    internal MessagePackage(MessageReceivedEventArgs args, ReceiveViewModel viewModel) 
        : this(args.Time, args.Sender, viewModel, args.Files, args.TextMessage)
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
    
    internal ReactiveCommand<Unit,Unit> DownloadZipCommand { get; }
    internal ReactiveCommand<Unit,Unit> DownloadToFolderCommand { get; }

    internal bool HasFiles => Files.Length > 0;

    public void NotifyPropertyChanged(string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void DownloadZip()
    {
        async Task ConfirmAction()
        {
            string result = await OpenDestinationFolder();
            string zipName = $"{Sender?.Nickname}_{Time.Year}-{Time.Month}-{Time.Day}_{Time.Hour}-{Time.Minute}-{Time.Second}";
            Utilities.CreateZip(result, zipName, Files);
        }
        
        string dialogTitle = $"Do you really want to download {Files.Length} files in a compressed ZIP-archive?";
        string[] buttonTexts = {"Yes, download ZIP!", "Cancel"};
        SetConfirmationDialog(dialogTitle, buttonTexts, ConfirmAction);
    }

    private void DownloadToFolder()
    {
        async Task ConfirmAction()
        {
            string result = await OpenDestinationFolder();
            Utilities.SaveFilesToFolder(Files, result);
        }
        
        string dialogTitle = $"Do you really want to download {Files.Length} files to a destination folder?";
        string[] buttonTexts = {"Yes, download files!", "Cancel"};
        SetConfirmationDialog(dialogTitle, buttonTexts, ConfirmAction);
    }

    private static async Task<string> OpenDestinationFolder()
    {
        if (MainWindow.Instance is not { } mainWindow) // TODO: Handle failure
            throw new InvalidOperationException("MainWindow.Instance is null and can't be used as a parent window for the dialog.");
            
        var fileDialog = new OpenFolderDialog
        {
            Title = "Select the destination folder"
        };
        
        string? result = await fileDialog.ShowAsync(mainWindow);
        return result ?? throw new InvalidOperationException("The path returned from the dialog is null.");
        // TODO: Handle failure if `result` is null
    }

    private void SetConfirmationDialog(string title, IEnumerable<string> buttonTexts, Func<Task> confirmAction)
    {
        if (_receiveViewModel is null)
            return; // TODO: Handle failure
        var dialog = new ConfirmationDialogViewModel(_receiveViewModel, title,
            new[] {Resources.MainRed, Resources.MainGrey},
            new[] {Colors.White, Colors.White}, buttonTexts, confirmAction);
        _receiveViewModel.CurrentDialog = dialog;
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