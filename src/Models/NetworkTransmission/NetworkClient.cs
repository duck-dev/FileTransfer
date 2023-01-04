using System;
using System.Collections.Generic;
using System.IO;
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

    internal async Task InvokeSendingPackageAsync(IList<FileObject> files, string message)
    {
        using Socket client = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        bool connectionEstablished = false;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                await client.ConnectAsync(IpEndPoint);
                connectionEstablished = true;
                break;
            }
            catch (SocketException e)
            {
                Utilities.Log($"SocketException in NetworkClient (try-catch ConnectAsync): {e}");
            }
        }

        if (!connectionEstablished)
        {
            // TODO: Handle failure
            return;
        }

        // Send own GUID
        if (Utilities.LocalUser is null)
            throw new UserNotFoundException("LocalUser could not be found.");
        Task<bool> sendTask = SendDataAsync(Utilities.LocalUser.UniqueGuid.ToByteArray(), client);
        await InvokeSendingAsync(sendTask);

        // Send length of message
        sendTask = SendSizeAsync(message.Length, client);
        await InvokeSendingAsync(sendTask);

        byte[] messageBytes;
        if (message.Length > 0)
        {
            // Send text message
            messageBytes = Encoding.UTF8.GetBytes(message);
            sendTask = SendDataAsync(messageBytes, client);
            await InvokeSendingAsync(sendTask);
        }

        // Send how many files
        sendTask = SendSizeAsync(files.Count, client);
        await InvokeSendingAsync(sendTask);

        if (files.Count > 0)
        {
            foreach (FileObject file in files)
            {
                // Send size of file
                long fileSize = file.FileInformation.Length;
                messageBytes = BitConverter.GetBytes(fileSize);
                sendTask = SendDataAsync(messageBytes, client);
                await InvokeSendingAsync(sendTask);

                // Send length of file name
                sendTask = SendSizeAsync(file.FileInformation.Name.Length, client);
                await InvokeSendingAsync(sendTask);

                // Send file name
                messageBytes = Encoding.UTF8.GetBytes(file.FileInformation.Name);
                sendTask = SendDataAsync(messageBytes, client);
                await InvokeSendingAsync(sendTask);

                // Send file
                await using var fileStream = new FileStream(file.FileInformation.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
                for (int i = 0; i < fileSize; i += BufferSize)
                {
                    messageBytes = new byte[BufferSize];
                    int received = await fileStream.ReadAsync(messageBytes, 0, messageBytes.Length);
                    sendTask = SendDataAsync(messageBytes, client);
                    await InvokeSendingAsync(sendTask);
                }
            }
        }
        
        // Close connection
        await client.DisconnectAsync(false);
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }

    private static async Task InvokeSendingAsync(Task<bool> task)
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