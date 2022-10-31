using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views;

public class MessagePackageView : UserControl
{
    public MessagePackageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}