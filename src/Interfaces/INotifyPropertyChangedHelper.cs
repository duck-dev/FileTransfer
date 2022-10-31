using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileTransfer.Interfaces;

public interface INotifyPropertyChangedHelper : INotifyPropertyChanged
{
    void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
}