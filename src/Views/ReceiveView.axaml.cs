using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

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
        MenuFlyout flyout = new MenuFlyout
        {
            Items = new []
            {
                new MenuItem { Header = "Download ZIP" },
                new MenuItem { Header = "Download to folder" }
            },
            Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
        };
        FlyoutBase.SetAttachedFlyout(button, flyout);
        FlyoutBase.ShowAttachedFlyout(button);
    }
}