using System.Net;
using FileTransfer.Models;

namespace FileTransfer.ViewModels;

public sealed class SendViewModel : ViewModelBase
{
    private int ReceiverIndex { get; set; }
    private string Message { get; set; } = string.Empty;
    // TODO: Add collection of files

    private async void SendAsync()
    {
        IPAddress ip = await UtilityCollection.Utilities.GetIpAddressAsync(); // TODO: Replace by selected receiver
        var client = new NetworkClient(ip);
        await client.InvokeSendingDataAsync(Message); // TODO: Pass files
    }
}