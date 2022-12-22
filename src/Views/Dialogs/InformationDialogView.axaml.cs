using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileTransfer.ViewModels.Dialogs;

namespace FileTransfer.Views.Dialogs;

public class InformationDialogView : UserControl
{
    public InformationDialogView()
    {
        InitializeComponent();
        this.Initialized += (sender, args) =>
        {
            if(this.DataContext is InformationDialogViewModel viewModel) 
                viewModel.InvokeInitializedView(this);
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}