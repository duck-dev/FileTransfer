using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;

namespace FileTransfer.ViewModels;

public sealed class ReceiveViewModel : ViewModelBase
{
    internal NetworkServer Server { get; }
    
    public ReceiveViewModel()
    {
        Server = new NetworkServer();
        Server.MessageReceived += OnMessageReceived;
    }
    
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // TODO: Do something
        UtilityCollection.Utilities.Log($"\nRECEIVED MESSAGE:");
        UtilityCollection.Utilities.Log($"From: {UtilityCollection.Utilities.UsersList.Find(x => x.UniqueGuid == e.SenderGuid)?.Nickname}");
        UtilityCollection.Utilities.Log($"Message: {e.TextMessage}\n");
    }
}