using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileTransfer.UtilityCollection;

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
        this.Closing += (sender, args) => Utilities.DeleteFilesInDirectory(Utilities.TemporaryFilesPath);
    }
    
    ~MainWindow() => Utilities.DeleteFilesInDirectory(Utilities.TemporaryFilesPath);

    internal static MainWindow? Instance { get; private set; }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}