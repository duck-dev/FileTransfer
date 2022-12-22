using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using FileTransfer.Interfaces;
using ReactiveUI;

namespace FileTransfer.ViewModels.Dialogs;

public class InformationDialogViewModel : DialogViewModelBase
{
    internal event EventHandler? OnViewInitialized;

    private object? _additionalContent;
    
    public InformationDialogViewModel(IDialogContainer dialogContainer, string title, IEnumerable<SolidColorBrush> buttonColors,
        IEnumerable<SolidColorBrush> buttonTextColors, IEnumerable<string> buttonTexts)
        : base(dialogContainer, title, buttonColors, buttonTextColors, buttonTexts)
    { }

    internal object? AdditionalContent
    {
        get => _additionalContent;
        set => this.RaiseAndSetIfChanged(ref _additionalContent, value);
    }
    
    internal void InvokeInitializedView(UserControl control) => OnViewInitialized?.Invoke(control, EventArgs.Empty);
}