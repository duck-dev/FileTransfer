using System;
using System.Net;
using System.Net.Sockets;
using FileTransfer.Exceptions;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private static readonly Random _random = new();
    
    internal static char Cipher(char input)
    {
        if (!char.IsNumber(input)) 
            return input;
        
        return (char)('9' - input + '0');
    }

    internal static string EncryptID(string username, IPAddress ipAddress)
    {
        int addressFamily = (int)ipAddress.AddressFamily;
        if (addressFamily is < 0 or > 99)
            throw new InvalidIpException("Invalid IP-address family.");
        
        string result = $"{username}_{addressFamily:D2}";

        string ipAddressString = ipAddress.ToString();
        foreach (char c in ipAddressString)
        {
            if (char.IsNumber(c))
            {
                result += Cipher(c);
            }
            else
            {
                int rnd = _random.Next(0, Letters.Length);
                result += Letters[rnd];
            }
        }
        return result;
    }
    
    internal static void DecryptID(string id, out string username, out string ipString)
    {
        // Separate username from the rest
        int separatorIndex = id.LastIndexOf('_');
        username = id.Substring(0, separatorIndex);
        
        // Get IP-Address family
        string familyCodeString = id.Substring(separatorIndex + 1, 2);
        AddressFamily family;
        if(int.TryParse(familyCodeString, out int familyCode))
            family = (AddressFamily)familyCode;
        else
            throw new InvalidCastException("Invalid AddressFamily code");

        if (family is not AddressFamily.InterNetwork && family is not AddressFamily.InterNetworkV6)
            throw new InvalidIpException("The provided IP address does not match one of the InterNetwork family types.");
        
        // Get IP-Address
        ipString = string.Empty;
        foreach (char c in id.Substring(separatorIndex + 3, id.Length - (separatorIndex + 3)))
        {
            if (char.IsNumber(c))
                ipString += Cipher(c);
            else
                ipString += ".";
        }
    }
}