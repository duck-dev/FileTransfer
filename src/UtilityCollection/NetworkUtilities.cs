using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FileTransfer.Exceptions;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    internal const int NormalPort = 31415;
    internal const int ContactCommunicationPort = 31416;
    internal const int CheckOnlineStatusPort = 31417;
    
    internal static async Task<IPAddress> GetIpAddressAsync()
    {
        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
        IPAddress? finalIp;
        if ((finalIp = ipAddresses.FirstOrDefault(ip => ip.AddressFamily is AddressFamily.InterNetworkV6)) != null)
            return finalIp;

        throw new InvalidIpException("No inter network IP address found.");
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
        
        await client.DisconnectAsync(false);
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }
}