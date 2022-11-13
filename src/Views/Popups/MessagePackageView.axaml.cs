using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FileTransfer.ViewModels;

namespace FileTransfer.Views;

public class MessagePackageView : UserControl
{
    public MessagePackageView()
    {
        InitializeComponent();
        this.Initialized += (sender, args) =>
        {
            Border copiedTextElement = this.Get<Border>("CopiedTextDisplay");
            copiedTextElement.IsVisible = false;
            if (this.DataContext is MessagePackageViewModel viewModel)
            {
                viewModel.CopiedText += (o, eventArgs) =>
                {
                    copiedTextElement.IsVisible = true;
                    Task.Delay(2800).ContinueWith(async x => await DisableCopiedTextElement(copiedTextElement));
                };
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private static async Task DisableCopiedTextElement(IVisual visual)
        => await Dispatcher.UIThread.InvokeAsync(() => visual.IsVisible = false);
}