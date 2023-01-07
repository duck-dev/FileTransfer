using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FileTransfer.CustomControls.SelectionTypeControls;
using FileTransfer.Enums;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels;

namespace FileTransfer.Views;

public class MessagePackageView : UserControl
{
    private const int MinTextSize = 87;

    private TextBox _textMessage = null!;
    
    public MessagePackageView()
    {
        InitializeComponent();
        this.Initialized += (sender, args) =>
        {
            SetupCopiedTextElement();
            InitTextFilesSpace();
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitTextFilesSpace()
    {
        _textMessage = this.Get<TextBox>("TextMessage");
        _textMessage.PropertyChanged += SetupTextFilesSpace;
    }

    private void SetupTextFilesSpace(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property != BoundsProperty)
            return;
        _textMessage.PropertyChanged -= SetupTextFilesSpace;
        ScrollViewer elementToResize = this.Get<ScrollViewer>("TextSpace");

        if (_textMessage.Bounds.Height > MinTextSize)
            elementToResize.MinHeight = MinTextSize;
        else
            elementToResize.MinHeight = _textMessage.Bounds.Height;

        Grid parentGrid = this.Get<Grid>("ParentGrid");
        const int autoIndex = 3;
        const int fixedIndex = 4;

        Utilities.LimitAutoSize(parentGrid, elementToResize, GridOrientation.Row, autoIndex, fixedIndex);
    }

    private void SetupCopiedTextElement()
    {
        Border copiedTextElement = this.Get<Border>("CopiedTextDisplay");
        copiedTextElement.IsVisible = false;
        if (this.DataContext is MessagePackageViewModel viewModel)
        {
            viewModel.CopiedText += (o, eventArgs) =>
            {
                copiedTextElement.IsVisible = true;
                Task.Delay(2800).ContinueWith(async x => await DisableCopiedTextElement(copiedTextElement));
            };
        }
    }

    private void ShowDownloadFlyout(Control button)
    {
        if (button.DataContext is SelectionTypeBase<UIFileObject> file)
            ShowFlyout(button, file.Reference.File.DownloadCommand);
    }

    private void ShowMultipleDownloadFlyout(Control button)
    {
        if (this.DataContext is MessagePackageViewModel viewModel)
            ShowFlyout(button, viewModel.DownloadSelectedFilesCommand);
    }

    private static void ShowFlyout(Control button, ICommand command)
    {
        var items = new MenuItem[]
        {
            // TODO: Enable when default path implemented (see: `FileObject.Download(bool)`)
            new() { Header = "Download to default folder", Command = command, CommandParameter = false, IsEnabled = false },
            new() { Header = "Download to selected location", Command = command, CommandParameter = true }
        };
        Utilities.ShowFlyout(button, items, FlyoutPlacementMode.BottomEdgeAlignedLeft);
    }
    
    private static async Task DisableCopiedTextElement(IVisual visual)
        => await Dispatcher.UIThread.InvokeAsync(() => visual.IsVisible = false);

    private void FileSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (this.DataContext is MessagePackageViewModel viewModel)
            viewModel.FileSelected(e.AddedItems, e.RemovedItems);
    }
}