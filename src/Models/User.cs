using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Avalonia.Media;
using FileTransfer.Interfaces;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models;

public class User : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private string _nickname = string.Empty;
    private string _id = string.Empty;
    private string _username = string.Empty;
    private string _initials = string.Empty;
    private bool _isOnline = false;
    
    [JsonConstructor]
    public User(string id, string nickname, uint colorCode)
    {
        this.ID = id;

        if (!Utilities.DecryptID(id, out string? username, out string? ipString))
            throw new InvalidDataException("Could not Decrypt ID (invalid User ID): `User` ctor.");
        this.Username = username!;
        this.Nickname = nickname;
        
        string[] wordsInNickname = Nickname.Split(char.Parse(" "), 2);
        this.Initials = wordsInNickname.Length == 1 ? Nickname[0].ToString() : $"{wordsInNickname[0][0]}{wordsInNickname[1][0]}";
        this.Initials = Initials.ToUpperInvariant();
        
        this.ColorCode = colorCode;
        Color color = Color.FromUInt32(colorCode);
        this.ColorBrush = new SolidColorBrush(color);
        
        if (IPAddress.TryParse(ipString, out IPAddress? tempIp))
            this.IP = tempIp;
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
        }
    }
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}