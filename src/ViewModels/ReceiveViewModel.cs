using System.Linq;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.UtilityCollection;

namespace FileTransfer.ViewModels;

public sealed class ReceiveViewModel : NetworkViewModelBase
{
    internal NetworkServer Server { get; }
    
    public ReceiveViewModel() : base(Utilities.UsersList)
    {
        Server = new NetworkServer();
        Server.MessageReceived += OnMessageReceived;
    }
    
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // TODO: Do something
        Utilities.Log($"\nRECEIVED MESSAGE:");
        Utilities.Log($"From: {UsersList?.FirstOrDefault(x => x.UniqueGuid == e.SenderGuid)?.Nickname}");
        Utilities.Log($"Message: {e.TextMessage}\n");
    }
}