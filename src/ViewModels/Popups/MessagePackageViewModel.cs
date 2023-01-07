using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using FileTransfer.Models;
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
    }
    
    internal MessagePackage Message { get; }
    
    internal UIFileObject[] Files { get; }

    internal int MinFilesHeight => Message.HasFiles ? 100 : 0;

    private bool SelectionEnabled
    {
        get => _selectionEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectionEnabled, value);
            foreach (UIFileObject file in Files)
                file.UpdateSelectionType(_selectionEnabled);
            _selectedFiles.Clear();
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
        
        UtilityCollection.Utilities.Log("------------------------------------");
        foreach(var x in _selectedFiles)
            UtilityCollection.Utilities.Log(x.FileInformation.Name);
    }
}