using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views.Popups;

public class MaximizedTextView : UserControl
{
    public MaximizedTextView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}