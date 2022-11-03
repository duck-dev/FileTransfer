using System;
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
    private DialogViewModelBase? _currentDialog;
    private string _message = string.Empty;

    public SendViewModel() : base(Utilities.UsersList)
    {
        FileNames.CollectionChanged += (sender, args) => this.RaisePropertyChanged(nameof(HasFiles));
    }
    
    public DialogViewModelBase? CurrentDialog
    {
        get => _currentDialog;
        set => this.RaiseAndSetIfChanged(ref _currentDialog, value);
    }

    internal RangeObservableCollection<string> FileNames { get; } = new();

    private bool HasFiles => FileNames.Count > 0;

    private int ReceiverIndex { get; set; }

    private string Message
    {
        get => _message; 
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    private void Send()
    {
        User? user = UsersList?[ReceiverIndex];
        
        async Task ConfirmAction()
        {
            if (user?.IP is not { } ip)
                throw new InvalidIpException("Selected user has a null IP.");
            var client = new NetworkClient(ip);
            await client.InvokeSendingDataAsync(Message); // TODO: Pass files
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
            new [] { Resources.MainRed, Resources.MainGrey },
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
        if (result != null)
            FileNames.AddRange(result);
    }

    private void Reset()
    {
        Message = string.Empty;
        // TODO: Reset files
    }
}