using System;
using System.Net;

namespace FileTransfer.Models;

internal class User
{
    internal User(byte[] guid, string username, string ipString)
    {
        this.UniqueGuid = new Guid(guid);
        this.Username = username;
        this.Nickname = username; // TODO: Replace by locally saved nickname for User with this GUID
        if (IPAddress.TryParse(ipString, out IPAddress? tempIp))
            this.IP = tempIp;
    }
    
    internal Guid UniqueGuid { get; }
    
    internal string Username { get; set; }
    internal string Nickname { get; set; }
    
    internal IPAddress? IP { get; set; }
}