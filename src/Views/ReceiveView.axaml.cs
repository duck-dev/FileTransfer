using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Views;

public class ReceiveView : UserControl
{
    public ReceiveView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    internal void ShowDownloadFlyout(Control button)
    {
        if (button.DataContext is not MessagePackage package)
            return;

        var items = new MenuItem[]
        {
            new() { Header = "Download ZIP", Command = package.DownloadZipCommand },
            new() { Header = "Download to folder", Command = package.DownloadToFolderCommand }
        };
        Utilities.ShowFlyout(button, items, FlyoutPlacementMode.BottomEdgeAlignedLeft);
    }
}