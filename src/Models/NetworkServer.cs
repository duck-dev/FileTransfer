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
        Task.Run(ReceiveData);
    }

    private async Task ReceiveData()
    {
        while (true)
        {
            using Socket listener = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(IpEndPoint);
            listener.Listen(100);

            Socket handler = await listener.AcceptAsync();
            while (true)
            {
                // Receive message
                var buffer = new byte[1024]; // TODO: Update size accordingly (receive overall size, calculate optimal buffer)
                int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.UTF8.GetString(buffer, 0, received);

                const string eom = "<|EOM|>";
                if (response.IndexOf(eom, StringComparison.Ordinal) <= -1) 
                    continue;
                
                var eventArgs = new MessageReceivedEventArgs
                {
                    Content = null, TextMessage = response, Received = received, Time = DateTime.Now
                };
                MessageReceived?.Invoke(this, eventArgs);

                UtilityCollection.Utilities.Log($"Socket server received message: \"{response.Replace(eom, string.Empty)}\"");

                // Send acknowledgement
                const string acknowledgement = "<|ACK|>";
                byte[] echoBytes = Encoding.UTF8.GetBytes(acknowledgement);
                await handler.SendAsync(echoBytes, 0);
                UtilityCollection.Utilities.Log($"Socket server sent acknowledgment: \"{acknowledgement}\"");
                break;
            }
        }
    }
}