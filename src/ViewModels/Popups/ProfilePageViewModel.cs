using System;
using System.Linq;
using Avalonia.Media;
using FileTransfer.Models;
using FileTransfer.ResourcesNamespace;
using FileTransfer.ViewModels.Dialogs;
using ReactiveUI;

namespace FileTransfer.ViewModels;

internal class ProfilePageViewModel : ViewModelBase
{
    private const string WhitespaceErrorText = "No whitespaces allowed.";
    private const string UsernameEqualErrorText = "The new username must differ from the current one.";
    
    private bool _isEditingUsername;
    private string _newUsername = string.Empty;

    private MetaData MetaDataInstance { get; set; } = ApplicationVariables.MetaData!;

    private bool IsEditingUsername
    {
        get => _isEditingUsername; 
        set => this.RaiseAndSetIfChanged(ref _isEditingUsername, value);
    }

    private bool IsUsernameValid => !UsernameContainsWhitespace && !NewUsername.Equals(MetaDataInstance.LocalUser?.Username);

    private bool UsernameContainsWhitespace => string.IsNullOrEmpty(NewUsername) || string.IsNullOrWhiteSpace(NewUsername) 
                                               || NewUsername.Any(char.IsWhiteSpace);

    private string NewUsername
    {
        get => _newUsername;
        set
        {
            this.RaiseAndSetIfChanged(ref _newUsername, value);
            this.RaisePropertyChanged(nameof(IsUsernameValid));
            this.RaisePropertyChanged(nameof(ErrorText));
        }
    }

    private string ErrorText => UsernameContainsWhitespace ? WhitespaceErrorText : UsernameEqualErrorText;

    private void EditUsername()
    {
        IsEditingUsername = true;
    }

    private void ConfirmChanges()
    {
        if (!IsUsernameValid)
        {
            DiscardChanges();
            return;
        }

        Action action = () =>
        {
            MetaDataInstance.LocalUser?.ChangeUsername(NewUsername);
            DiscardChanges();
        };
        
        if (ReceiveViewModel.Instance is not { } receiveViewModel)
        {
            action();
        }
        else
        {
            string dialogTitle = $"Do you want to change the username to '{NewUsername}'?";
            receiveViewModel.CurrentDialog = new ConfirmationDialogViewModel(receiveViewModel, dialogTitle,
                new[] { Resources.MainRed, Resources.MainGrey },
                new[] { Colors.White, Colors.White },
                new[] { "Yes", "Cancel" },
                action);
        }
    }
    
    private void DiscardChanges()
    {
        IsEditingUsername = false;
        NewUsername = string.Empty;
    }
}