using FileTransfer.ViewModels;

namespace FileTransfer.Interfaces;

public interface IDialogContainer
{
    public DialogViewModelBase? CurrentDialog { get; set; }
}