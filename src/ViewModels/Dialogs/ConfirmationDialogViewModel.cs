using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;

namespace FileTransfer.ViewModels.Dialogs;

public class ConfirmationDialogViewModel : DialogViewModelBase
{
    private enum ActionType
    {
        Confirm,
        Cancel
    }

    private readonly Action? _confirmAction;
    private readonly Action? _cancelAction;
    private readonly Func<Task>? _confirmActionAsync;

    public ConfirmationDialogViewModel(IDialogContainer dialogContainer, string title, IEnumerable<SolidColorBrush> buttonColors,
        IEnumerable<SolidColorBrush> buttonTextColors, IEnumerable<string> buttonTexts, Action? confirmAction, Action? cancelAction = null) 
        : base(dialogContainer, title, buttonColors, buttonTextColors, buttonTexts)
    {
        _confirmAction = confirmAction;
        _cancelAction = cancelAction;
    }

    public ConfirmationDialogViewModel(IDialogContainer dialogContainer, string title, IEnumerable<Color> buttonColors,
        IEnumerable<Color> buttonTextColors, IEnumerable<string> buttonTexts, Action? confirmAction, Action? cancelAction = null) 
        : this(dialogContainer, title, 
            buttonColors.Select(x => new SolidColorBrush(x)), 
            buttonTextColors.Select(x => new SolidColorBrush(x)), 
            buttonTexts, confirmAction, cancelAction) { }
    
    public ConfirmationDialogViewModel(IDialogContainer dialogContainer, string title, IEnumerable<SolidColorBrush> buttonColors,
        IEnumerable<SolidColorBrush> buttonTextColors, IEnumerable<string> buttonTexts, Func<Task>? confirmAction, Action? cancelAction = null) 
        : base(dialogContainer, title, buttonColors, buttonTextColors, buttonTexts)
    {
        _confirmActionAsync = confirmAction;
        _cancelAction = cancelAction;
    }

    public ConfirmationDialogViewModel(IDialogContainer dialogContainer, string title, IEnumerable<Color> buttonColors,
        IEnumerable<Color> buttonTextColors, IEnumerable<string> buttonTexts, Func<Task>? confirmAction, Action? cancelAction = null) 
        : this(dialogContainer, title, 
            buttonColors.Select(x => new SolidColorBrush(x)), 
            buttonTextColors.Select(x => new SolidColorBrush(x)), 
            buttonTexts, confirmAction, cancelAction) { }

    private async Task Command(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Confirm:
                //IgnoreDialog(); // Set a variable in the settings that makes sure this dialog will be ignored in the future
                _confirmAction?.Invoke();
                if (_confirmActionAsync != null) 
                    await _confirmActionAsync();
                break;
            case ActionType.Cancel:
                _cancelAction?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }

        CloseDialog();
    }
}