using FileTransfer.ViewModels;

namespace FileTransfer;

public interface IDialogContainer
{
    public DialogViewModelBase? CurrentDialog { get; set; }
}