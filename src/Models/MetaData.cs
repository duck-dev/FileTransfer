using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using FileTransfer.Interfaces;

namespace FileTransfer.Models;

public class MetaData : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private User? _localUser;
    
    [JsonConstructor]
    public MetaData(bool isFirstLogin, User? localUser, ObservableCollection<User>? usersList)
    {
        this.IsFirstLogin = isFirstLogin;
        this.LocalUser = localUser;
        this.UsersList = usersList;
    }
    
    public bool IsFirstLogin { get; set; }

    public User? LocalUser
    {
        get => _localUser;
        set
        {
            _localUser = value;
            NotifyPropertyChanged();
        }
    }

    public ObservableCollection<User>? UsersList { get; set; }
    
    // TODO: Set to default download location set in the settings (default: "Downloads" folder) by default
    internal string? RecentDownloadLocation { get; set; } // = DEFAULT_LOCATION
    
    // TODO: Set to default upload location set in the settings (default: YET TO BE DEFINED) by default
    internal string? RecentUploadLocation { get; set; } // = DEFAULT_LOCATION
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}