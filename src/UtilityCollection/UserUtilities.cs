using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using FileTransfer.Enums;
using FileTransfer.Exceptions;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal static event EventHandler<User>? OnUserOnlineStatusChanged; 
    
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Random _random = new();

    private static readonly Color[] _userColors =
    {
        Color.Parse("#FFAE00"), Color.Parse("#FF8800"), Color.Parse("#C92E0C"), Color.Parse("#FF0000"), Color.Parse("#AF0000"),
        Color.Parse("#680000"), Color.Parse("#680040"), Color.Parse("#300061"), Color.Parse("#5200A3"), Color.Parse("#9933FF"), 
        Color.Parse("#B82EB3"), Color.Parse("#D92B6E"), Color.Parse("#3396FF"), Color.Parse("#0071EB"), Color.Parse("#0059B8"),
        Color.Parse("#00356E"), Color.Parse("#1447FF"), Color.Parse("#002FD9"), Color.Parse("#001052"), Color.Parse("#124727"),
        Color.Parse("#426B2B"), Color.Parse("#2CB05F"), Color.Parse("#146359"), Color.Parse("#1E9482"), Color.Parse("#B7612F"),
        Color.Parse("#6B4016"), Color.Parse("#522300"), Color.Parse("#4D4D4D"), Color.Parse("#000000")
    };
    
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
        char delimiter = DetermineIPDelimiter(ipAddress.AddressFamily);
        foreach (char c in ipAddressString)
        {
            if (char.IsNumber(c))
            {
                result += Cipher(c);
            }
            else if(c == delimiter)
            {
                int rnd = _random.Next(0, Letters.Length);
                result += Letters[rnd];
            } else if (char.IsLetter(c))
            {
                result += c;
            }
        }
        return result;
    }
    
    internal static bool DecryptID(string id, out string? username, out string? ipString)
    {
        username = null;
        ipString = null;
        
        // Separate username from the rest
        int separatorIndex = id.LastIndexOf('_');
        if (separatorIndex < 0)
            return false;
        username = id.Substring(0, separatorIndex);
        
        // Get IP-Address family
        if (id.Length < separatorIndex + 3)
            return false;
        string familyCodeString = id.Substring(separatorIndex + 1, 2);
        AddressFamily family;
        if (int.TryParse(familyCodeString, out int familyCode))
        {
            family = (AddressFamily) familyCode;
        }
        else
        {
            Log($"DecryptID: Invalid AddressFamily code ({(AddressFamily)familyCode}[{familyCode}]).");
            return false;
        }

        if (family is not AddressFamily.InterNetwork && family is not AddressFamily.InterNetworkV6)
        {
            Log("The provided IP address does not match one of the InterNetwork family types.");
            return false;
        }
        
        // Get IP-Address
        int startIndex = separatorIndex + 3;
        int length = id.Length - startIndex;
        ipString = string.Empty;
        char delimiter = DetermineIPDelimiter(family);
        foreach (char c in id.Substring(startIndex, length))
        {
            if (char.IsNumber(c))
                ipString += Cipher(c);
            else if (char.IsUpper(c))
                ipString += delimiter;
            else
                ipString += c;
        }
        return true;
    }

    private static char DetermineIPDelimiter(AddressFamily addressFamily)
    {
        if (addressFamily is AddressFamily.InterNetwork)
            return '.';
        else if (addressFamily is AddressFamily.InterNetworkV6)
            return ':';
        else
            throw new InvalidIpException("Invalid IP-address family.");
    }

    internal static async Task<Tuple<bool, User?>> IsIDValid(string id)
    {
        User? user = null;
        if (!DecryptID(id, out string? username, out string? ipString) || !IPAddress.TryParse(ipString, out IPAddress? ip)) 
            return new Tuple<bool, User?>(false, user);

        IPEndPoint endPoint = new IPEndPoint(ip, ContactCommunicationPort);
        using Socket client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        bool establishedConnection = await EstablishConnection(endPoint, client);
        if (!establishedConnection)
        {
            CloseConnection(client);
            return new Tuple<bool, User?>(false, user);
        }
        
        await SendCommunicationCode(CommunicationCode.CheckUsername, client);
        
        // Receive correct username
        byte[] buffer = new byte[30];
        int received = await client.ReceiveAsync(buffer, SocketFlags.None);
        string correctUsername = Encoding.UTF8.GetString(buffer, 0, received);
        // Send acknowledgement
        await NetworkServer.SendAcknowledgementAsync(client);
        if (!correctUsername.Equals(username))
        {
            int separatorIndex = id.LastIndexOf('_');
            if (separatorIndex < 0)
            {
                Log("Invalid ID!");
                // Close connection
                CloseConnection(client);
                return new Tuple<bool, User?>(false, user);
            }

            id = $"{correctUsername}{id.Substring(separatorIndex, id.Length - separatorIndex)}";
            username = correctUsername;
        }

        user = new User(id, username, GetRandomUserColor().ToUint32());
        
        // Close connection
        CloseConnection(client);
        
        return new Tuple<bool, User?>(true, user);
    }

    internal static async Task<bool> CheckUserOnline(User user)
    {
        if (user.IP is not { } ip)
            return false;
        
        IPEndPoint endPoint = new IPEndPoint(ip, CheckOnlineStatusPort);
        using Socket client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        bool establishedConnection = await EstablishConnection(endPoint, client);
        if (!establishedConnection)
            return false;

        Task<Tuple<bool, int>> sendTask = NetworkClient.SendDataAsync(new byte[1] { (byte)CommunicationCode.UpdateOnlineStatus }, client);
        await NetworkClient.InvokeSendingAsync(sendTask);
        
        CloseConnection(client);
        return true;
    }
    
    internal static void RaiseUpdateOnlineStatusEvent(User user) => OnUserOnlineStatusChanged?.Invoke(null, user);

    private static async Task SendCommunicationCode(CommunicationCode code, Socket client)
    {
        // Send own ID
        Task<Tuple<bool, int>> sendTask = NetworkClient.SendDataAsync(Encoding.UTF8.GetBytes(ApplicationVariables.MetaData.LocalUser!.ID), client);
        await NetworkClient.InvokeSendingAsync(sendTask);
        
        // Send communication code
        sendTask = NetworkClient.SendDataAsync(new byte[1] {(byte)code}, client);
        await NetworkClient.InvokeSendingAsync(sendTask);
    }
    
    internal static Color GetRandomUserColor()
    {
        int index = _random.Next(0, _userColors.Length);
        return _userColors[index];
    }
}