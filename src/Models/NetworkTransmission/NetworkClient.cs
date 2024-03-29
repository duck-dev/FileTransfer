using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.ResourcesNamespace;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels;
using FileTransfer.ViewModels.Dialogs;

namespace FileTransfer.Models.NetworkTransmission;

internal class NetworkClient : NetworkObject
{
    internal NetworkClient(IPAddress? ipAddress = null) : base(ipAddress) { }

    internal async Task InvokeSendingPackageAsync(IList<FileObject> files, string message, User user, SendViewModel viewModel)
    {
        using Socket client = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        bool establishedConnection = await Utilities.EstablishConnection(IpEndPoint, client);
        if (!establishedConnection)
        {
            Utilities.Log(":( NetworkClient: FAILED TO ESTABLISH CONNECTION... :(");
            if (ReceiveViewModel.Instance is not { } receiveViewModel)
                return;
            
            string dialogTitle = $"The connection could not be established. {user.Nickname} could be offline. Try again later!";
            receiveViewModel.CurrentDialog = new InformationDialogViewModel(receiveViewModel, dialogTitle, new [] { Resources.AppPurpleBrush}, 
                new [] { Resources.WhiteBrush }, new [] { "Ok!" });
            return;
        }
        
        viewModel.LoadingTitle = files.Count > 0 ? $"Sending {files.Count} files to {user.Nickname}" : $"Sending message to {user.Nickname}";
        long overallBytes = files.Sum(x => x.FileInformation.Length) + message.Length;
        (float byteDivisor, string unit) = overallBytes switch
        {
            < 1000 => (1f, "B"),
            < 1_000_000 => (1_000f, "KB"),
            < 1_000_000_000 => (1_000_000f, "MB"),
            _ => (1_000_000_000f, "GB")
        };
        UpdateProgress(0, overallBytes, byteDivisor, unit, viewModel);
        viewModel.IsSending = true;

        // Send own ID
        if (MetaDataInstance.LocalUser is null)
            throw new UserNotFoundException("LocalUser could not be found.");
        Task<Tuple<bool, int>> sendTask = SendDataAsync(Encoding.UTF8.GetBytes(MetaDataInstance.LocalUser.ID), client);
        await InvokeSendingAsync(sendTask);

        // Send length of message
        sendTask = SendSizeAsync(message.Length, client);
        await InvokeSendingAsync(sendTask);

        byte[] messageBytes;
        long sentBytes = 0;
        if (message.Length > 0)
        {
            // Send text message
            messageBytes = Encoding.UTF8.GetBytes(message);
            sendTask = SendDataAsync(messageBytes, client);
            sentBytes += await InvokeSendingAsync(sendTask);
            UpdateProgress(sentBytes, overallBytes, byteDivisor, unit, viewModel);
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
                    int tempBufferSize = BufferSize;
                    if (fileSize - BufferSize < i)
                        tempBufferSize = (int)(fileSize - i);
                    messageBytes = new byte[tempBufferSize];
                    int received = await fileStream.ReadAsync(messageBytes, 0, messageBytes.Length);
                    sendTask = SendDataAsync(messageBytes, client);
                    sentBytes += await InvokeSendingAsync(sendTask);
                    UpdateProgress(sentBytes, overallBytes, byteDivisor, unit, viewModel);
                }
            }
        }
        
        // Close connection
        await client.DisconnectAsync(false);
        client.Shutdown(SocketShutdown.Both);
        client.Close();

        await Task.Delay(2000);
        viewModel.IsSending = false;
    }

    internal static async Task<int> InvokeSendingAsync(Task<Tuple<bool, int>> task)
    {
        while (true)
        {
            var (receivedAcknowledgement, item2) = await task;
            if (receivedAcknowledgement)
                return item2;
        }
    }

    internal static async Task<Tuple<bool, int>> SendSizeAsync(int messageLength, Socket client)
    {
        byte[] lengthBytes = BitConverter.GetBytes(messageLength);
        return await SendDataAsync(lengthBytes, client);
    }

    internal static async Task<Tuple<bool, int>> SendDataAsync(byte[] buffer, Socket client)
    {
        int bytesSent = await client.SendAsync(buffer, SocketFlags.None);
        bool receivedAcknowledgement = await ReceivedAcknowledgementAsync(client);
        return Tuple.Create(receivedAcknowledgement, bytesSent);
    }

    private static async Task<bool> ReceivedAcknowledgementAsync(Socket client)
    {
        var buffer = new byte[8];
        int received = await client.ReceiveAsync(buffer, SocketFlags.None);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        return response == "<|ACK|>";
    }

    private static void UpdateProgress(long sentBytes, long overallBytes, float byteDivisor, string unit, SendViewModel viewModel)
    {
        viewModel.SendingProgress = $"{(int)(sentBytes / (float)overallBytes * 100)}%";
        viewModel.LoadingSubtitle = $"{(sentBytes/byteDivisor):F1} {unit} of {Utilities.DataSizeRepresentation(overallBytes)} transmitted";
    }
}