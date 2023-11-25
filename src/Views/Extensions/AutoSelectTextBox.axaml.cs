using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace FileTransfer.Views.Extensions;

public class AutoSelectTextBox : TextBox, IStyleable
{
    public AutoSelectTextBox()
    {
        InitializeComponent();
        this.Initialized += (sender, args) => this.Focus();
        this.PropertyChanged += (sender, args) =>
        {
            if (args.Property == IsVisibleProperty && IsVisible)
                this.Focus();
        };
    }
    
    Type IStyleable.StyleKey => typeof(TextBox);
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}