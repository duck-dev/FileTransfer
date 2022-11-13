using System.Collections.ObjectModel;
using Avalonia.Media;
using FileTransfer.Events;
using FileTransfer.Interfaces;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.ResourcesNamespace;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels.Dialogs;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public sealed class ReceiveViewModel : NetworkViewModelBase, IDialogContainer
{
    private MessagePackageViewModel? _messageViewModel;
    private DialogViewModelBase? _currentDialog;
    
    public DialogViewModelBase? CurrentDialog
    {
        get => _currentDialog;
        set => this.RaiseAndSetIfChanged(ref _currentDialog, value);
    }
    
    internal NetworkServer Server { get; }
    
    private ObservableCollection<MessagePackage> Messages { get; } = new();
    private bool MessagesAvailable => Messages.Count > 0;

    private MessagePackageViewModel? MessageViewModel
    {
        get => _messageViewModel; 
        set => this.RaiseAndSetIfChanged(ref _messageViewModel, value);
    }
    
    public ReceiveViewModel() : base(Utilities.UsersList)
    {
        Server = new NetworkServer();
        Server.MessageReceived += OnMessageReceived;
        Messages.CollectionChanged += (sender, args) => this.RaisePropertyChanged(nameof(MessagesAvailable));
    }
    
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        var message = new MessagePackage(args);
        Messages.Insert(0, message);
        Utilities.Log("\nRECEIVED MESSAGE:");
        Utilities.Log($"From: {message.Sender?.Nickname}");
        Utilities.Log($"Message: {message.TextMessage}\n");
    }
    
    private void SetMessage(MessagePackage? message)
    {
        if (message is null)
        {
            MessageViewModel = null;
            return;
        }
        
        message.IsRead = true;
        MessageViewModel = new MessagePackageViewModel(message);
    }

    private void DeleteMessage(MessagePackage? message)
    {
        if (message is null)
            return;
        
        // if (dialogDisabled)
        // {
        //     Messages.Remove(message)
        //     return;
        // }

        string dialogTitle = $"Are you sure you want to delete this message from '{message.Sender?.Nickname}'?";
        CurrentDialog = new ConfirmationDialogViewModel(this, dialogTitle,
            new [] { Resources.MainRed, Resources.MainGrey },
            new[] { Colors.White, Colors.White },
            new[] { "Yes, delete message!", "Cancel" },
            () => Messages.Remove(message));
    }
}