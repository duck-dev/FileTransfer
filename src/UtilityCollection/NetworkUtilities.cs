using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FileTransfer.Exceptions;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal const int NormalPort = 31415;
    internal const int ContactCommunicationPort = 31416;
    internal const int CheckOnlineStatusPort = 31417;

    private static string _ipKey = string.Empty;

    internal static string IPKey
    {
        get => _ipKey;
        set
        {
            if (_ipKey == string.Empty) // Can only be set once, assuming the default value of the `_ipKey` field is `string.Empty`
                _ipKey = value;
        }
    }

    // TODO: Temporary for testing
    internal static async Task<IPAddress> GetLocalIpAddressAsync()
    {
        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
        IPAddress? finalIp;
        if ((finalIp = ipAddresses.FirstOrDefault(ip => ip.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6)) != null)
            return finalIp;

        throw new InvalidIpException("No inter network IP address found.");
    }
    
    internal static async Task<IPAddress> GetIpAddressAsync()
    {
        try
        {
            HttpClient client = new HttpClient();
            string ip = await client.GetStringAsync($"https://api.whatismyip.com/ip.php?key={IPKey}");
            if (IPAddress.TryParse(ip, out IPAddress? ipAddress))
                return ipAddress;
            throw new InvalidIpException("Invalid IP-address obtained!");
        }
        catch (Exception e)
        {
            Log($"Couldn't get IP-address: {e}");
            throw;
        }
    }

    internal static async Task<bool> EstablishConnection(IPEndPoint endPoint, Socket client)
    {
        try
        {
            IAsyncResult? result = default;
            await Task.Run(() =>
            {
                result = client.BeginConnect(endPoint, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(5000, true);
            }).WaitAsync(new TimeSpan(0,0,0,0,5000));
            if (!client.Connected || result is null) 
                return false;
            client.EndConnect(result);
            return true;
        }
        catch (Exception e)
        {
            Log($"Exception in NetworkClient (try-catch Connect): {e}");
        }

        return false;
    }

    internal static async Task CloseConnection(Socket? client)
    {
        if (client is null)
            return;
        
        if(client.Connected)
            await client.DisconnectAsync(false);
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }
}