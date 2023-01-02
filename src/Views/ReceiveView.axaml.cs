using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using FileTransfer.Models;

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

    internal void ToggleContextMenu(Control button)
    {
        if (button.DataContext is not MessagePackage package)
            return;
        
        MenuFlyout flyout = new MenuFlyout
        {
            Items = new []
            {
                new MenuItem { Header = "Download ZIP", Command = package.DownloadZipCommand },
                new MenuItem { Header = "Download to folder", Command = package.DownloadToFolderCommand }
            },
            Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
        };
        FlyoutBase.SetAttachedFlyout(button, flyout);
        FlyoutBase.ShowAttachedFlyout(button);
    }
}