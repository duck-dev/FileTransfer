using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Models;

internal class NetworkServer : NetworkObject
{
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    internal NetworkServer(IPAddress? ipAddress = null) : base(ipAddress)
    {
        Task.Run(ReceiveDataAsync);
    }

    private async Task ReceiveDataAsync()
    {
        while (true)
        {
            using Socket listener = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(IpEndPoint);
            listener.Listen(100);

            Socket handler = await listener.AcceptAsync();
            while (true)
            {
                // Receive length
                var buffer = new byte[4];
                _ = await handler.ReceiveAsync(buffer, SocketFlags.None);
                int messageLength = BitConverter.ToInt32(buffer, 0);
                
                // Send Acknowledgement
                await SendAcknowledgementAsync(handler);

                int handledBytes = 0;
                string response = string.Empty;
                int received = 0;
                while (handledBytes < messageLength)
                {
                    // Receive message
                    buffer = new byte[1024];
                    received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    response += Encoding.UTF8.GetString(buffer, 0, received);
                    handledBytes += received;
                }
                UtilityCollection.Utilities.Log($"Socket server received message: \"{response}\"");

                // Send acknowledgement
                await SendAcknowledgementAsync(handler);
                
                var eventArgs = new MessageReceivedEventArgs
                {
                    Content = null, TextMessage = response, Received = received, Time = DateTime.Now
                };
                MessageReceived?.Invoke(this, eventArgs);
                break;
            }
        }
    }

    private static async Task SendAcknowledgementAsync(Socket handler)
    {
        const string acknowledgement = "<|ACK|>";
        byte[] echoBytes = Encoding.UTF8.GetBytes(acknowledgement);
        await handler.SendAsync(echoBytes, 0);
        UtilityCollection.Utilities.Log("Socket server sent acknowledgment: \"<|ACK|>\"");
    }
}