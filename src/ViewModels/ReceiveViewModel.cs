using System.Collections.ObjectModel;
using FileTransfer.Events;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.UtilityCollection;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public sealed class ReceiveViewModel : NetworkViewModelBase
{
    private MessagePackageViewModel? _messageViewModel;
    
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
    
    private void OpenMessage(MessagePackage message)
    {
        message.IsRead = true;
        MessageViewModel = new MessagePackageViewModel(message);
    }
}