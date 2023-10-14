using System.Net;
using Avalonia.Media;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models;

public class User
{
    internal User(string id, string nickname, uint colorCode)
    {
        this.ID = id;
        
        Utilities.DecryptID(id, out string username, out string ipString);
        this.Username = username;
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
    
    public string Nickname { get; set; }
    public string ID { get; set; }
    public uint ColorCode { get; set; } // For JSON
    
    internal string Username { get; set; }
    internal string Initials { get; set; }
    internal SolidColorBrush ColorBrush { get; set; }
    internal IPAddress? IP { get; set; }
    internal bool IsOnline { get; set; }
}