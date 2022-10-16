using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Models;

internal class NetworkClient : NetworkObject
{
    internal NetworkClient(IPAddress? ipAddress = null) : base(ipAddress) { }

    internal async Task InvokeSendingDataAsync()
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
            const string message = "Hello World!<|EOM|>"; // TODO: Replace by files and optional text message
            
            // Send length
            int messageLength = message.Length; // TODO: Overall size of entire content including optional text message
            byte[] lengthBytes = BitConverter.GetBytes(messageLength);
            bool receivedAcknowledgement = await SendDataAsync(lengthBytes, client);
            // Receive acknowledgement => continue
            if(!receivedAcknowledgement)
                continue;

            // Send message
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            receivedAcknowledgement = await SendDataAsync(messageBytes, client);

            // Receive acknowledgement => terminate operation
            if (!receivedAcknowledgement) 
                continue;
            break;
        }
        client.Shutdown(SocketShutdown.Both);
        await client.DisconnectAsync(false);
        client.Close();
    }

    private static async Task<bool> SendDataAsync(byte[] buffer, Socket client)
    {
        _ = await client.SendAsync(buffer, SocketFlags.None);
        UtilityCollection.Utilities.Log($"Socket client sent data!");
        return await ReceivedAcknowledgementAsync(client);
    }

    private static async Task<bool> ReceivedAcknowledgementAsync(Socket client)
    {
        var buffer = new byte[8];
        int received = await client.ReceiveAsync(buffer, SocketFlags.None);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        UtilityCollection.Utilities.Log($"Socket client received acknowledgment: \"<|ACK|>\"");
        return response == "<|ACK|>";
    }
}