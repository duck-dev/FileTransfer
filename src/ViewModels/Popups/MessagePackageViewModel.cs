using FileTransfer.Models;

namespace FileTransfer.ViewModels;

internal class MessagePackageViewModel : ViewModelBase
{
    public MessagePackageViewModel(MessagePackage message)
        => this.Message = message;
    
    private MessagePackage Message { get; }
}