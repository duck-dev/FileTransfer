using System;
using System.Net;
using Avalonia.Media;

namespace FileTransfer.Models;

public class User
{
    internal User(byte[] guid, string username, string ipString, Color color)
    {
        this.UniqueGuid = new Guid(guid); // TODO: Replace with ID
        this.UniqueGuidStr = UniqueGuid.ToString(); // TODO: Replace with ID
        this.Username = username;
        this.Nickname = username; // TODO: Replace by locally saved nickname for User with this GUID
        
        string[] wordsInNickname = Nickname.Split(char.Parse(" "), 2);
        this.Initials = wordsInNickname.Length == 1 ? Nickname[0].ToString() : $"{wordsInNickname[0][0]}{wordsInNickname[1][0]}";
        this.Initials = Initials.ToUpperInvariant();
        
        this.Color = new SolidColorBrush(color);
        if (IPAddress.TryParse(ipString, out IPAddress? tempIp))
            this.IP = tempIp;
    }
    
    internal Guid UniqueGuid { get; } // TODO: Replace with ID
    internal string UniqueGuidStr { get; } // TODO: Replace with ID
    
    internal string Username { get; set; }
    internal string Nickname { get; set; }
    internal string Initials { get; set; }
    internal SolidColorBrush Color { get; set; }
    
    internal IPAddress? IP { get; set; }
    
    internal bool IsOnline { get; set; }
}