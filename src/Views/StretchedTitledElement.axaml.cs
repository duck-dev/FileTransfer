using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views;

// Can't inherit `TitledElement` as some elements fail to display without a specific reason
public class StretchedTitledElement : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TitledElement, string>(nameof(Title));
    public static readonly StyledProperty<object?> InnerContentProperty =
        AvaloniaProperty.Register<TitledElement, object?>(nameof(InnerContent));
    
    public StretchedTitledElement()
    {
        InitializeComponent();
    }
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public object? InnerContent
    {
        get => GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}