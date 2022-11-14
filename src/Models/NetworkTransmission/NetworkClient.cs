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

    internal async Task InvokeSendingPackageAsync(IEnumerable<FileObject> files, string message)
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

        // Send own GUID
        if (Utilities.LocalUser is null)
            throw new UserNotFoundException("LocalUser could not be found.");
        Task<bool> sendTask = SendDataAsync(Utilities.LocalUser.UniqueGuid.ToByteArray(), client);
        await InvokeSendingAsync(sendTask, client);
        
        // Send length of message
        sendTask = SendSizeAsync(message.Length, client);
        await InvokeSendingAsync(sendTask, client);
        
        // Send text message
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        sendTask = SendDataAsync(messageBytes, client);
        await InvokeSendingAsync(sendTask, client);
        
        // TODO: Send how many files
        // TODO: Send size of files (if no files, send 0)
        // TODO: Send files (if no files, send "empty" byte array)
        // TODO: For sending files: Send each file individually in a loop and at the end send "end of message" indicator (find better way than string)
        
        // Close connection
        client.Shutdown(SocketShutdown.Both);
        await client.DisconnectAsync(false);
        client.Close();
    }

    private static async Task InvokeSendingAsync(Task<bool> task, Socket client)
    {
        while (true)
        {
            bool receivedAcknowledgement = await task;
            if (receivedAcknowledgement)
                break;
        }
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