using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using FileTransfer.Extensions;
using FileTransfer.Interfaces;

namespace FileTransfer.ViewModels;

public abstract class DialogViewModelBase : ViewModelBase
{
    private readonly IDialogContainer _dialogContainer;
    
    protected DialogViewModelBase(IDialogContainer dialogContainer, string title, IEnumerable<SolidColorBrush> buttonColors,
        IEnumerable<SolidColorBrush> buttonTextColors, IEnumerable<string> buttonTexts)
    {
        _dialogContainer = dialogContainer;
        this.Title = title;
        this.ButtonColors = buttonColors.ToArray();
        this.ButtonTextColors = buttonTextColors.ToArray();
        this.ButtonTexts = buttonTexts.ToArray();
    }
    
    protected string Title { get; init; }
    
    protected SolidColorBrush[] ButtonColors { get; }
    protected SolidColorBrush[] ButtonColorsHover
        => ButtonColors.Select(x => new SolidColorBrush(x.Color.DarkenColor(0.1f))).ToArray();
    protected SolidColorBrush[] ButtonTextColors { get; }
    
    protected string[] ButtonTexts { get; }
    
    protected bool IgnoreDialog { get; set; }
    
    protected void CloseDialog()
    {
        _dialogContainer.CurrentDialog = null;
    }
}