using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileTransfer.ViewModels;

namespace FileTransfer.Views;

public class NotificationsView : UserControl
{
    public NotificationsView()
    {
        InitializeComponent();
        this.PropertyChanged += async (sender, args) =>
        {
            if (args.Property != IsVisibleProperty)
                return;

            if (!IsVisible || this.DataContext is not NotificationsViewModel viewModel)
                return;

            await Task.Delay(500);
            viewModel.MarkAllAsRead();
        };
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}