using FileTransfer.Models;

namespace FileTransfer.ViewModels;

internal class MessagePackageViewModel : ViewModelBase
{
    public MessagePackageViewModel(MessagePackage message)
        => this.Message = message;
    
    internal MessagePackage Message { get; }
}