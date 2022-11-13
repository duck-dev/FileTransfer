using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models.NetworkTransmission;

internal class NetworkClient : NetworkObject
{
    internal NetworkClient(IPAddress? ipAddress = null) : base(ipAddress) { }

    internal async Task InvokeSendingDataAsync(IEnumerable<FileObject> files, string message)
    {
        using Socket client = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        try
        {
            await client.ConnectAsync(IpEndPoint);
        }
        catch (SocketException e)
        {
            Utilities.Log(e.Message);
            // TODO: Handle failure
            return;
        }

        bool sentGuid = false;
        while (true)
        {
            // Send own GUID
            if (Utilities.LocalUser is null)
                throw new UserNotFoundException("LocalUser could not be found.");
            
            bool receivedAcknowledgement = sentGuid || await SendDataAsync(Utilities.LocalUser.UniqueGuid.ToByteArray(), client);
            if(!receivedAcknowledgement)
                continue;
            sentGuid = true;

            // Send length of message
            receivedAcknowledgement = await SendSizeAsync(message.Length, client);
            if(!receivedAcknowledgement)
                continue;

            // Send text message
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            receivedAcknowledgement = await SendDataAsync(messageBytes, client);
            if (!receivedAcknowledgement) 
                continue;
            
            // TODO: Send size of files (if no files, send 0)
            // TODO: Send files (if no files, send "empty" byte array)
            // TODO: For sending files: Send each file individually in a loop and at the end send "end of message" indicator (find better way than string)
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
        return await ReceivedAcknowledgementAsync(client);
    }

    private static async Task<bool> ReceivedAcknowledgementAsync(Socket client)
    {
        var buffer = new byte[8];
        int received = await client.ReceiveAsync(buffer, SocketFlags.None);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        return response == "<|ACK|>";
    }
}