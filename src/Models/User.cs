using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Media;
using FileTransfer.Interfaces;
using FileTransfer.Services;
using FileTransfer.UtilityCollection;
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace FileTransfer.Models;

public class User : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private string _nickname = string.Empty;
    private string _id = string.Empty;
    private string _username = string.Empty;
    private string _initials = string.Empty;
    private bool _isOnline = false;
    private bool _isEditingContact;

    private readonly Timer _onlineStatusTimer = new(10_000);
    
    [JsonConstructor]
    public User(string id, string nickname, uint colorCode, bool isLocalUser = false)
    {
        this.ID = id;

        if (!Utilities.DecryptID(id, out string? username, out string? ipString))
            throw new InvalidDataException("Could not Decrypt ID (invalid User ID): `User` ctor.");
        this.Username = username!;
        this.Nickname = nickname;

        UpdateInitials();
        
        this.ColorCode = colorCode;
        Color color = Color.FromUInt32(colorCode);
        this.ColorBrush = new SolidColorBrush(color);
        
        if (IPAddress.TryParse(ipString, out IPAddress? tempIp))
            this.IP = tempIp;

        this.IsLocalUser = isLocalUser;

        if (isLocalUser)
        {
            this.IsOnline = true;
            return;
        }
        
        _onlineStatusTimer.Elapsed += async (_, _) => await CheckUserOnline();
        _onlineStatusTimer.AutoReset = true;
        _onlineStatusTimer.Start();

        Task.Run(async () => await CheckUserOnline());
    }
    
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            NotifyPropertyChanged();
        }
    }

    public string ID
    {
        get => _id;
        set
        {
            _id = value;
            NotifyPropertyChanged();
        }
    }
    public uint ColorCode { get; set; } // For JSON
    
    public bool IsLocalUser { get; }

    internal string Username
    {
        get => _username;
        set
        {
            _username = value;
            NotifyPropertyChanged();
        }
    }

    internal string Initials
    {
        get => _initials;
        set
        {
            _initials = value;
            NotifyPropertyChanged();
        }
    }
    internal SolidColorBrush ColorBrush { get; set; }
    internal IPAddress? IP { get; set; }

    internal bool IsOnline
    {
        get => _isOnline; 
        set
        {
            _isOnline = value;
            NotifyPropertyChanged();
            
            if (!IsLocalUser)
                Utilities.RaiseUpdateOnlineStatusEvent(this);
        }
    }

    internal string NewNickname { get; set; } = string.Empty;

    internal bool IsEditingContact
    {
        get => _isEditingContact;
        set
        {
            if (value == _isEditingContact)
                return;
            _isEditingContact = value;
            NotifyPropertyChanged();
        }
    }
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal async Task CheckUserOnline()
    {
        bool userOnline = await Utilities.CheckUserOnline(this);
        IsOnline = userOnline;
    }

    internal void ConfirmChanges()
    {
        string oldNickname = Nickname;
        if (string.IsNullOrEmpty(NewNickname) || string.IsNullOrWhiteSpace(NewNickname))
            Nickname = Username;
        else
            Nickname = NewNickname;

        if (Nickname.Equals(oldNickname))
        {
            DiscardChanges();
            return;
        }
        
        UpdateInitials();
        DiscardChanges();
        DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
    }

    internal void DiscardChanges()
    {
        IsEditingContact = false;
        NewNickname = string.Empty;
    }

    private void UpdateInitials()
    {
        string[] wordsInNickname = Nickname.Split(char.Parse(" "), 2);
        this.Initials = wordsInNickname.Length == 1 ? Nickname[0].ToString() : $"{wordsInNickname[0][0]}{wordsInNickname[1][0]}";
        this.Initials = Initials.ToUpperInvariant();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not User otherUser)
            return false;

        bool ipEquality = this.IP?.Equals(otherUser.IP) ?? this.IP == otherUser.IP;
        return otherUser.Username.Equals(this.Username) && ipEquality;
    }
}