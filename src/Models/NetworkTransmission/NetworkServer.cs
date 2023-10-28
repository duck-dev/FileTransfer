using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FileTransfer.Enums;
using FileTransfer.Events;
using FileTransfer.Exceptions;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models.NetworkTransmission;

internal class NetworkServer : NetworkObject
{
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    internal NetworkServer(IPAddress? ipAddress = null) : base(ipAddress)
    {
        Task.Run(ReceiveDataAsync);
        Task.Run(ContactCommunicationAsync);
    }

    private async Task ReceiveDataAsync()
    {
        using Socket listener = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Listen(listener, IpEndPoint);
        
        while (true)
        {
            using Socket handler = await listener.AcceptAsync();
            
            // Receive sender
            var buffer = new byte[BufferSize];
            int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            string senderID = Encoding.UTF8.GetString(buffer, 0, received);
            // Send Acknowledgement
            await SendAcknowledgementAsync(handler);

            // Receive message length
            buffer = new byte[4];
            _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
            int messageLength = BitConverter.ToInt32(buffer, 0);
            // Send Acknowledgement
            await SendAcknowledgementAsync(handler);

            int handledBytes = 0;
            string textMessage = string.Empty;
            received = 0;
            if (messageLength > 0)
            {
                while (handledBytes < messageLength)
                {
                    // Receive message
                    buffer = new byte[BufferSize];
                    received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    textMessage += Encoding.UTF8.GetString(buffer, 0, received);
                    handledBytes += received;
                }
                // Send acknowledgement
                await SendAcknowledgementAsync(handler);
            }

            // Receive file count
            buffer = new byte[4];
            _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
            int fileCount = BitConverter.ToInt32(buffer, 0);
            // Send Acknowledgement
            await SendAcknowledgementAsync(handler);

            List<FileObject>? files = null;
            if (fileCount > 0)
            {
                DateTime now = DateTime.Now;
                string nowString = $"{now.Year}-{now.Month}-{now.Day}_{now.Hour}-{now.Minute}-{now.Second}-{now.Millisecond}";
                string directory = Path.Combine(Utilities.TemporaryFilesPath, senderID, nowString);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                
                files = new List<FileObject>();
                // Receive files
                for (int i = 0; i < fileCount; i++)
                {
                    // Get file size
                    buffer = new byte[8];
                    _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    long fileSize = BitConverter.ToInt64(buffer, 0);
                    // Send Acknowledgement
                    await SendAcknowledgementAsync(handler);
                    
                    // Get size of file name
                    buffer = new byte[4];
                    _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    messageLength = BitConverter.ToInt32(buffer, 0);
                    // Send Acknowledgement
                    await SendAcknowledgementAsync(handler);
                    
                    // Get file name
                    handledBytes = 0;
                    string fileName = string.Empty;
                    received = 0;
                    while (handledBytes < messageLength)
                    {
                        // Receive message
                        buffer = new byte[BufferSize];
                        received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                        fileName += Encoding.UTF8.GetString(buffer, 0, received);
                        handledBytes += received;
                    }
                    // Send acknowledgement
                    await SendAcknowledgementAsync(handler);
 
                    string path = Path.Combine(directory, fileName);
                    if (File.Exists(path))
                        path = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(path)} - Copy{Path.GetExtension(path)}");
                    await using var fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write, BufferSize, true);
                    for (long j = 0; j < fileSize; j += BufferSize)
                    {
                        int tempBufferSize = BufferSize;
                        if (fileSize - BufferSize < j)
                            tempBufferSize = (int)(fileSize - j);
                        buffer = new byte[tempBufferSize];
                        _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
                        await fileStream.WriteAsync(buffer, 0, buffer.Length);

                        // Send Acknowledgement
                        await SendAcknowledgementAsync(handler);
                    }
                    
                    await fileStream.FlushAsync();
                    fileStream.Close();
                    
                    files.Add(new FileObject(path));
                }
            }
            
            if (MetaDataInstance.UsersList is null)
                throw new Exception("UsersList is null!");
            User? sender = MetaDataInstance.UsersList.FirstOrDefault(x => x.ID == senderID);
            if (sender is null && senderID == MetaDataInstance.LocalUser?.ID) // For development and testing
                sender = MetaDataInstance.LocalUser;
            else
                throw new UserNotFoundException($"User with ID {senderID} could not be found.");
            
            FileObject[] filesArray = files == null ? Array.Empty<FileObject>() : files.ToArray();
            var eventArgs = new MessageReceivedEventArgs
            {
                Files = filesArray, TextMessage = textMessage, Time = DateTime.Now, Sender = sender
            };
            MessageReceived?.Invoke(this, eventArgs);
            
            // Close connection of the `handler`
            // DO NOT close or disconnect the `listener`
            await handler.DisconnectAsync(false);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    private async Task ContactCommunicationAsync()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Utilities.ContactCommunicationPort);
        using Socket listener = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Listen(listener, endPoint);

        while (true)
        {
            using Socket handler = await listener.AcceptAsync();
            
            // Receive sender
            var buffer = new byte[BufferSize];
            int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            string senderID = Encoding.UTF8.GetString(buffer, 0, received);
            // Send Acknowledgement
            await SendAcknowledgementAsync(handler);
            bool decryptedID = Utilities.DecryptID(senderID, out string? decryptedUsername, out string? decryptedIP);
            
            // Receive communication code
            buffer = new byte[1];
            received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            byte communicationCode = buffer[0];
            // Send Acknowledgement
            await SendAcknowledgementAsync(handler);

            switch ((CommunicationCode)communicationCode)
            {
                case CommunicationCode.CheckUsername:
                    Task<Tuple<bool, int>> sendTask = NetworkClient.SendDataAsync(Encoding.UTF8.GetBytes(ApplicationVariables.MetaData.LocalUser!.Username), handler);
                    await NetworkClient.InvokeSendingAsync(sendTask);
                    break;
                case CommunicationCode.UsernameChanged:
                    if (!decryptedID || ApplicationVariables.MetaData.UsersList?.FirstOrDefault(x => x.IP is {} ip && ip.ToString().Equals(decryptedIP)) is not { } userUsernameChanged)
                        break;
                    userUsernameChanged.Username = decryptedUsername!;
                    break;
                case CommunicationCode.IPChanged:
                    if (!decryptedID || ApplicationVariables.MetaData.UsersList?.FirstOrDefault(x => x.Username is {} username && username.Equals(decryptedUsername)) is not { } userIPChanged)
                        break;
                    try
                    {
                        userIPChanged.IP = IPAddress.Parse(decryptedIP!);
                    }
                    catch (Exception e)
                    {
                        Utilities.Log($"Could not parse IP ({decryptedIP}): {e}");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Close connection of the `handler`
            // DO NOT close or disconnect the `listener`
            await handler.DisconnectAsync(false);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    internal static async Task SendAcknowledgementAsync(Socket handler)
    {
        const string acknowledgement = "<|ACK|>";
        byte[] echoBytes = Encoding.UTF8.GetBytes(acknowledgement);
        await handler.SendAsync(echoBytes, 0);
    }

    private void Listen(Socket listener, EndPoint endPoint)
    {
        listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        try
        {
            listener.Bind(endPoint);
            listener.Listen(100);
        }
        catch (Exception e)
        {
            Utilities.Log($"Exception while binding and listening in NetworkServer.ReceiveDataAsync(): {e}");
            throw;
        }
    }
}