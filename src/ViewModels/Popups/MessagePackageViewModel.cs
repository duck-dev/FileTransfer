using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;
using ReactiveUI;

namespace FileTransfer.ViewModels;

internal class MessagePackageViewModel : ViewModelBase
{
    internal event EventHandler? CopiedText;

    private readonly List<FileObject> _selectedFiles = new();

    private bool _selectionEnabled;

    public MessagePackageViewModel(MessagePackage message)
    {
        this.Message = message;
        this.Files = message.Files.Select(x => new UIFileObject(x)).ToArray();
        this.DownloadSelectedFilesCommand = ReactiveCommand.Create<bool, Task>(DownloadSelectedFiles);
    }
    
    internal MessagePackage Message { get; }
    
    internal UIFileObject[] Files { get; }

    internal int MinFilesHeight => Message.HasFiles ? 100 : 0;
    
    private int SelectedFilesCount => _selectedFiles.Count;
    private bool AnyFilesSelected => SelectionEnabled && SelectedFilesCount > 0;
    
    internal ReactiveCommand<bool, Task> DownloadSelectedFilesCommand { get; }

    private bool SelectionEnabled
    {
        get => _selectionEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectionEnabled, value);
            foreach (UIFileObject file in Files)
            {
                file.Selected = false;
                file.UpdateSelectionType(_selectionEnabled);
            }
            _selectedFiles.Clear();
            
            this.RaisePropertyChanged(nameof(AnyFilesSelected));
        }
    }

    internal async Task CopyToClipboard()
    {
        if (Message.TextMessage is null || Application.Current is null || Application.Current.Clipboard is null)
            return;
        await Application.Current.Clipboard.SetTextAsync(Message.TextMessage);
        CopiedText?.Invoke(this, EventArgs.Empty);
    }

    internal void FileSelected(IList addedItems, IList removedItems)
    {
        foreach (var added in addedItems)
            FileSelected(added);

        foreach (var removed in removedItems)
            FileSelected(removed);
    }

    internal void FileSelected(object? obj)
    {
        if (obj is not UIFileObject file)
            return;

        file.Selected = !_selectedFiles.Contains(file.File);
        if (!_selectedFiles.Contains(file.File))
            _selectedFiles.Add(file.File);
        else
            _selectedFiles.Remove(file.File);
        
        this.RaisePropertyChanged(nameof(AnyFilesSelected));
        this.RaisePropertyChanged(nameof(SelectedFilesCount));
    }
    
    private async Task DownloadSelectedFiles(bool showDialog)
    {
        if (_selectedFiles.Count <= 0)
            return;
        
        const string title = "Select a destination";
        string? directory = ApplicationVariables.RecentDownloadLocation;

        string? location = null; // TODO: Replace `null` with default location set in the settings (default: "Downloads" folder)
        if (showDialog)
            location = await Utilities.InvokeOpenFolderDialog(title, directory);
        
        if (location != null)
            Utilities.SaveFilesToFolder(_selectedFiles, location);

        SelectionEnabled = false;
    }

    private void EnableSelectionAndSelect(UIFileObject file)
    {
        SelectionEnabled = true;
        FileSelected(file);
    }

    private void ToggleFilesSelection() => SelectionEnabled = !SelectionEnabled;
}