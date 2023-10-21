using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views.Popups;

public class WelcomeScreenView : UserControl
{
    public WelcomeScreenView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}