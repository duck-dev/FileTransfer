using System;
using System.Threading.Tasks;
using Avalonia.Media;
using FileTransfer.Exceptions;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.ResourcesNamespace;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels.Dialogs;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public sealed class SendViewModel : NetworkViewModelBase, IDialogContainer
{
    private DialogViewModelBase? _currentDialog;

    public SendViewModel() : base(Utilities.UsersList) { }
    
    public DialogViewModelBase? CurrentDialog
    {
        get => _currentDialog;
        set => this.RaiseAndSetIfChanged(ref _currentDialog, value);
    }
    
    private int ReceiverIndex { get; set; }
    private string Message { get; set; } = string.Empty;
    // TODO: Add collection of files

    private void Send()
    {
        async Task ConfirmAction()
        {
            if (UsersList?[ReceiverIndex].IP is not { } ip)
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

        const string dialogTitle = $"Are you sure you want to transmit the uploaded content to [USER]?"; // TODO: Add nickname of selected receiver instead of [USER]
        CurrentDialog = new ConfirmationDialogViewModel(this, dialogTitle,
            new [] { Resources.MainRed, Resources.MainGrey },
            new[] { Colors.White, Colors.White },
            new[] { "Yes, send data!", "Cancel" },
            (Func<Task>) ConfirmAction);
    }

    private void Reset()
    {
        Message = string.Empty;
        // TODO: Reset files
    }
}