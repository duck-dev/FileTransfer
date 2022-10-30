using System;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Media;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.ResourcesNamespace;
using FileTransfer.ViewModels.Dialogs;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public sealed class SendViewModel : ViewModelBase, IDialogContainer
{
    private DialogViewModelBase? _currentDialog;
    
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
            IPAddress ip = await UtilityCollection.Utilities.GetIpAddressAsync(); // TODO: Replace by selected receiver
            var client = new NetworkClient(ip);
            await client.InvokeSendingDataAsync(Message); // TODO: Pass files
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
}