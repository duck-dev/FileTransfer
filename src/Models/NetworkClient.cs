using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Models;

internal class NetworkClient : NetworkObject
{
    internal NetworkClient(IPAddress? ipAddress = null) : base(ipAddress) { }

    internal async Task SendData()
    {
        using Socket client = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        try
        {
            await client.ConnectAsync(IpEndPoint);
        }
        catch (SocketException e)
        {
            UtilityCollection.Utilities.Log(e.Message);
            // TODO: Handle failure
            return;
        }
        while (true)
        {
            // Send message
            const string message = "Hello World!<|EOM|>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
            UtilityCollection.Utilities.Log($"Socket client sent message: \"{message}\"");

            // Receive acknowledgement
            var buffer = new byte[1024];
            int received = await client.ReceiveAsync(buffer, SocketFlags.None);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response != "<|ACK|>") 
                continue;
            
            UtilityCollection.Utilities.Log($"Socket client received acknowledgment: \"{response}\"");
            break;
        }
        client.Shutdown(SocketShutdown.Both);
        await client.DisconnectAsync(false);
        client.Close();
    }
}