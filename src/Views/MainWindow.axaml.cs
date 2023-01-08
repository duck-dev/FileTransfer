using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels;

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
        this.Closing += (sender, args) => Utilities.DeleteDirectory(Utilities.TemporaryFilesPath);

        MainWindowViewModel.NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight, MaxItems = 3
        };
    }
    
    ~MainWindow() => Utilities.DeleteDirectory(Utilities.TemporaryFilesPath);

    internal static MainWindow? Instance { get; private set; }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}