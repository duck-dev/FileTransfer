using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Models;

internal class NetworkClient : NetworkObject
{
    internal NetworkClient(IPAddress? ipAddress = null) : base(ipAddress) { }

    internal async Task InvokeSendingDataAsync(string message) // TODO: Add parameter for files
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
            // Send length of message
            bool receivedAcknowledgement = await SendSizeAsync(message.Length, client);
            // Receive acknowledgement => continue
            if(!receivedAcknowledgement)
                continue;

            // Send text message
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            receivedAcknowledgement = await SendDataAsync(messageBytes, client);
            // Receive acknowledgement => terminate operation
            if (!receivedAcknowledgement) 
                continue;
            
            // TODO: Send size of files (if no files, send 0)
            // TODO: Send files (if no files, send "empty" byte array)
            break;
        }
        client.Shutdown(SocketShutdown.Both);
        await client.DisconnectAsync(false);
        client.Close();
    }

    private static async Task<bool> SendSizeAsync(int messageLength, Socket client)
    {
        byte[] lengthBytes = BitConverter.GetBytes(messageLength);
        return await SendDataAsync(lengthBytes, client);
    }

    private static async Task<bool> SendDataAsync(byte[] buffer, Socket client)
    {
        _ = await client.SendAsync(buffer, SocketFlags.None);
        UtilityCollection.Utilities.Log("Socket client sent data!");
        return await ReceivedAcknowledgementAsync(client);
    }

    private static async Task<bool> ReceivedAcknowledgementAsync(Socket client)
    {
        var buffer = new byte[8];
        int received = await client.ReceiveAsync(buffer, SocketFlags.None);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        UtilityCollection.Utilities.Log("Socket client received acknowledgment: \"<|ACK|>\"");
        return response == "<|ACK|>";
    }
}