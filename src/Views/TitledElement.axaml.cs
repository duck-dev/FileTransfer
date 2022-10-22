using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTransfer.Views;

public class TitledElement : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TitledElement, string>(nameof(Title));
    public static readonly StyledProperty<object?> InnerContentProperty =
        AvaloniaProperty.Register<TitledElement, object?>(nameof(InnerContent));
    
    public TitledElement()
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