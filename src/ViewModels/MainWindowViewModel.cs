using FileTransfer.Models;

namespace FileTransfer.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    internal NetworkServer Server { get; }
    
    public MainWindowViewModel()
    {
        Server = new NetworkServer();
        Server.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // TODO: Do something
        UtilityCollection.Utilities.Log($"\nRECEIVED MESSAGE: {e.TextMessage}\n");
    }
}