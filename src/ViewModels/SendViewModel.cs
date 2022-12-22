using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FileTransfer.Exceptions;
using FileTransfer.Extensions.ExtendedTypes;
using FileTransfer.Interfaces;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.ResourcesNamespace;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels.Dialogs;
using FileTransfer.Views;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public sealed class SendViewModel : NetworkViewModelBase, IDialogContainer
{
    private const int MaxSizePerFile = 2_000_000_000; // 2 GB
    
    private DialogViewModelBase? _currentDialog;
    private string _message = string.Empty;
    private int _receiverIndex = -1;

    public SendViewModel() : base(Utilities.UsersList)
    {
        Files.CollectionChanged += (sender, args) =>
        {
            this.RaisePropertyChanged(nameof(HasFiles));
            this.RaisePropertyChanged(nameof(FileCount));
            this.RaisePropertyChanged(nameof(SendingEnabled));
        };
    }
    
    public DialogViewModelBase? CurrentDialog
    {
        get => _currentDialog;
        set => this.RaiseAndSetIfChanged(ref _currentDialog, value);
    }

    internal RangeObservableCollection<FileObject> Files { get; } = new();
    
    internal string[]? LargeFilesNames { get; private set; }

    private bool HasFiles => Files.Count > 0;
    private int FileCount => Files.Count;

    private int ReceiverIndex
    {
        get => _receiverIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _receiverIndex, value);
            this.RaisePropertyChanged(nameof(SendingEnabled));
        }
    }

    private bool SendingEnabled => ReceiverIndex >= 0 && (Files.Count > 0 || !string.IsNullOrEmpty(Message));

    private string Message
    {
        get => _message;
        set
        {
            this.RaiseAndSetIfChanged(ref _message, value);
            this.RaisePropertyChanged(nameof(SendingEnabled));
        }
    }
    
    internal void EvaluateFiles(IEnumerable<string> result)
    {
        List<string>? largeFiles = null;
        foreach (string path in result)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > MaxSizePerFile)
            {
                largeFiles ??= new List<string>();
                largeFiles.Add(fileInfo.Name);
                continue;
            }
            Files.Add(new FileObject(path));
        }

        if (largeFiles is null || largeFiles.Count <= 0)
            return;
        LargeFilesNames = largeFiles.ToArray();
        
        string dialogTitle = $"The following files cannot be sent, because they are larger than {MaxSizePerFile / 1_000_000_000} GB:";
        var dialog = new InformationDialogViewModel(this, dialogTitle, new SolidColorBrush[] { Resources.AppPurpleBrush}, 
            new SolidColorBrush[]{ Resources.WhiteBrush }, new string[] { "Understood!" });
        dialog.OnViewInitialized += (sender, args) =>
        {
            if (sender is not UserControl control) 
                return;
            object? resource = Utilities.GetResourceFromStyle<object, UserControl>(control, "FilesTooLargeDialog", 1);
            dialog.AdditionalContent = resource;
        };
        CurrentDialog = dialog;
    }

    private void Send()
    {
        User? user = UsersList?[ReceiverIndex];
        
        async Task ConfirmAction()
        {
            if (user?.IP is not { } ip)
                throw new InvalidIpException("Selected user has a null IP.");
            var client = new NetworkClient(ip);
            await client.InvokeSendingPackageAsync(Files, Message);
            Reset();
        }

        // TODO: Turn method into async Task: `private async Task Send()`
        // if (dialogDisabled)
        // {
        //     await ConfirmAction();
        //     return;
        // }

        string dialogTitle = $"Are you sure you want to transmit the uploaded content to {user?.Nickname}?";
        CurrentDialog = new ConfirmationDialogViewModel(this, dialogTitle,
            new[] { Resources.MainRed, Resources.MainGrey },
            new[] { Colors.White, Colors.White },
            new[] { "Yes, send data!", "Cancel" },
            (Func<Task>) ConfirmAction);
    }

    private async Task BrowseFiles()
    {
        if (MainWindow.Instance is not { } mainWindow)
            return;
        
        var fileDialog = new OpenFileDialog
        {
            AllowMultiple = true, Title = "Select one or multiple files"
        };
        
        string[]? result = await fileDialog.ShowAsync(mainWindow);
        if (result is null)
            return;
        
        EvaluateFiles(result);
    }

    private void RemoveFile(FileObject file)
    {
        Files.Remove(file);
    }

    private void Reset()
    {
        ReceiverIndex = -1;
        Message = string.Empty;
        Files.Clear();
    }
}