using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileTransfer.CustomControls.SelectionTypeControls;
using FileTransfer.Interfaces;

namespace FileTransfer.Models;

internal class UIFileObject : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly SelectionTypeOne<UIFileObject> _selectionEnabledType;
    private readonly SelectionTypeTwo<UIFileObject> _selectionDisabledType;

    private bool _selected;
    private SelectionTypeBase<UIFileObject>? _selectionType;

    internal UIFileObject(FileObject file)
    {
        File = file;
        _selectionEnabledType = new SelectionTypeOne<UIFileObject>(this);
        _selectionDisabledType = new SelectionTypeTwo<UIFileObject>(this);
        SelectionType = _selectionDisabledType;
    }

    internal FileObject File { get; }
    
    internal bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            NotifyPropertyChanged();
        }
    }
    
    private SelectionTypeBase<UIFileObject>? SelectionType
    {
        get => _selectionType;
        set
        {
            _selectionType = value;
            NotifyPropertyChanged();
        }
    }
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal void UpdateSelectionType(bool selectionEnabled)
        => SelectionType = selectionEnabled ? _selectionEnabledType : _selectionDisabledType;
}