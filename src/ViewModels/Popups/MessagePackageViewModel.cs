using System.Threading.Tasks;
using Avalonia;
using FileTransfer.Models;

namespace FileTransfer.ViewModels;

internal class MessagePackageViewModel : ViewModelBase
{
    public MessagePackageViewModel(MessagePackage message)
        => this.Message = message;
    
    internal MessagePackage Message { get; }

    internal async Task CopyToClipboard()
    {
        if (Message.TextMessage is null || Application.Current is null || Application.Current.Clipboard is null)
            return;
        await Application.Current.Clipboard.SetTextAsync(Message.TextMessage);
    }
}