using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FileTransfer.ViewModels;

namespace FileTransfer.Views;

public class SendView : UserControl
{
    public SendView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DragOver(object? sender, DragEventArgs args)
    {
        UtilityCollection.Utilities.Log("DragOver");
        args.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;
        if (!args.Data.Contains(DataFormats.FileNames))
            args.DragEffects = DragDropEffects.None;
    }

    private void Drop(object? sender, DragEventArgs args)
    {
        UtilityCollection.Utilities.Log("Drop");
        if (!args.Data.Contains(DataFormats.FileNames) || this.DataContext is not SendViewModel viewModel ||
            args.Data.GetFileNames() is not { } fileNames)
        {
            return;
        }
        
        viewModel.EvaluateFiles(fileNames);
    }
}