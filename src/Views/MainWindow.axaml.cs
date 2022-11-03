using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views;

public class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        Instance = this;
    }
    
    internal static MainWindow? Instance { get; private set; }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}