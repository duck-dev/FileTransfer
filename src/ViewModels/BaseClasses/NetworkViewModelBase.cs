using System.Collections.ObjectModel;
using FileTransfer.Models;

namespace FileTransfer.ViewModels;

public abstract class NetworkViewModelBase : ViewModelBase
{
    protected ObservableCollection<User>? UsersList { get; }

    protected NetworkViewModelBase(ObservableCollection<User>? usersList) 
        => UsersList = usersList;
}