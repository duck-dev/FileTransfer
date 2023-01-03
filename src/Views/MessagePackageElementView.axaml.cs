using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FileTransfer.Models;

namespace FileTransfer.Views;

public class MessagePackageElementView : UserControl
{
    private Grid _fixedSizeGrid = null!;
    private ColumnDefinition _autoColumn = null!;
    private ColumnDefinition _fixedColumn = null!;
    private TextBlock _nicknameText = null!;
    
    public MessagePackageElementView()
    {
        InitializeComponent();
        this.Initialized += OnInitialized;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnInitialized(object? sender, EventArgs e)
    {
        _fixedSizeGrid = this.Get<Grid>("FixedSizeGrid");
        if (_fixedSizeGrid.IsInitialized)
        {
            GridOnInitialized(sender, EventArgs.Empty);
            return;
        }
        
        _fixedSizeGrid.Initialized += GridOnInitialized;
    }

    private void GridOnInitialized(object? sender, EventArgs e)
    {
        if (this.DataContext is MessagePackage package)
            package.TimeChanged += async (o, args) => await UpdateColumns(true);
        
        _autoColumn = _fixedSizeGrid.ColumnDefinitions[0]; // 1* at the beginning
        _fixedColumn = _fixedSizeGrid.ColumnDefinitions[1]; // Auto at the beginning
        _nicknameText = this.Get<TextBlock>("SenderNickname");
        
        _fixedSizeGrid.PropertyChanged += async (o, args) =>
        {
            if (args.Property == BoundsProperty)
                await UpdateColumns();
        };
    }

    private async Task UpdateColumns(bool callFromUIThread = false)
    {
        if (callFromUIThread)
            await Dispatcher.UIThread.InvokeAsync(ResetColumns);
        else
            ResetColumns();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _nicknameText.MaxWidth = _autoColumn.ActualWidth;
            // Switch ColumnDefinitions into the correct/desired order
            _autoColumn.Width = GridLength.Auto;
            _fixedColumn.Width = new GridLength(1, GridUnitType.Star);
        });
    }

    private void ResetColumns()
    {
        _fixedColumn.Width = GridLength.Auto;
        _autoColumn.Width = new GridLength(1, GridUnitType.Star);
    }
}