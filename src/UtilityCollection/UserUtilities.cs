using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using FileTransfer.Enums;
using FileTransfer.Exceptions;
using FileTransfer.Models;
using FileTransfer.Models.NetworkTransmission;
using FileTransfer.ResourcesNamespace;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal static event EventHandler<User>? OnUserOnlineStatusChanged; 
    
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
            Log("Invalid AddressFamily code.");
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
        foreach (char c in id.Substring(startIndex, length))
        {
            if (char.IsNumber(c))
                ipString += Cipher(c);
            else
                ipString += ".";
        }
        return true;
    }

    internal static async Task<Tuple<bool, User?>> IsIDValid(string id)
    {
        User? user = null;
        if (!DecryptID(id, out string? username, out string? ipString) || !IPAddress.TryParse(ipString, out IPAddress? ip)) 
            return new Tuple<bool, User?>(false, user);

        Tuple<bool, Socket?> tuple = await SendCommunicationCode(ip, CommunicationCode.CheckUsername);
        if (tuple.Item1 == false || tuple.Item2 is not {} client)
            return new Tuple<bool, User?>(false, user);
                
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
                await CloseConnection(client);
                return new Tuple<bool, User?>(false, user);
            }

            id = $"{correctUsername}{id.Substring(separatorIndex, id.Length - separatorIndex - 1)}";
            username = correctUsername;
        }

        await Dispatcher.UIThread.InvokeAsync(() => user = new User(id, username, Resources.MainRed.ToUint32())); // TODO: Random color
        
        // Close connection
        await CloseConnection(client);
        
        return new Tuple<bool, User?>(user is not null, user);
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
        
        await CloseConnection(client);
        return true;
    }
    
    internal static void RaiseUpdateOnlineStatusEvent(User user) => OnUserOnlineStatusChanged?.Invoke(null, user);

    private static async Task<Tuple<bool, Socket?>> SendCommunicationCode(User user, CommunicationCode code)
    {
        if (user.IP is null)
            return new Tuple<bool, Socket?>(false, null);
        return await SendCommunicationCode(user.IP, code);
    }

    private static async Task<Tuple<bool, Socket?>> SendCommunicationCode(IPAddress ip, CommunicationCode code)
    {
        IPEndPoint endPoint = new IPEndPoint(ip, ContactCommunicationPort);
        using Socket client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        bool establishedConnection = await EstablishConnection(endPoint, client);
        if (!establishedConnection)
            return new Tuple<bool, Socket?>(false, null);
        
        // Send own ID
        Task<Tuple<bool, int>> sendTask = NetworkClient.SendDataAsync(Encoding.UTF8.GetBytes(ApplicationVariables.MetaData.LocalUser!.ID), client);
        await NetworkClient.InvokeSendingAsync(sendTask);
        
        // Send communication code
        sendTask = NetworkClient.SendDataAsync(new byte[1] {(byte)code}, client);
        await NetworkClient.InvokeSendingAsync(sendTask);

        return new Tuple<bool, Socket?>(true, client);
    }
}