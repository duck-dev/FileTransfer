using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FileTransfer.Interfaces;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models;

public class MetaData : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private User? _localUser;
    
    [JsonConstructor]
    public MetaData(bool isFirstLogin, User? localUser, ObservableCollection<User>? usersList, List<User>? usersAddedMeList)
    {
        this.IsFirstLogin = isFirstLogin;
        this.LocalUser = localUser;
        this.UsersList = usersList ?? new ObservableCollection<User>();
        this.UsersAddedMeList = usersAddedMeList ?? new List<User>();

        if (LocalUser is null) 
            return;
        
        IPAddress? ownIP = null;
        Task.Run(async () => ownIP = await Utilities.GetLocalIpAddressAsync()).Wait(); // TODO: Change to public IP address
        if ((LocalUser.IP is null || !LocalUser.IP.Equals(ownIP)) && ownIP is not null)
            this.LocalUser.ChangeIP(ownIP);
            
        foreach (User user in UsersAddedMeList)
            user.CheckLocalUserData();
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

    public ObservableCollection<User> UsersList { get; set; }
    
    public List<User> UsersAddedMeList { get; set; }
    
    // TODO: Set to default download location set in the settings (default: "Downloads" folder) by default
    internal string? RecentDownloadLocation { get; set; } // = DEFAULT_LOCATION
    
    // TODO: Set to default upload location set in the settings (default: YET TO BE DEFINED) by default
    internal string? RecentUploadLocation { get; set; } // = DEFAULT_LOCATION
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}