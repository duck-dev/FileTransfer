using System.Net;

namespace FileTransfer.Models.NetworkTransmission;

internal abstract class NetworkObject
{
    private const int Port = 31415;
    
    protected NetworkObject(IPAddress? ipAddress = null)
    {
        ipAddress ??= IPAddress.Any;
        IpEndPoint = new IPEndPoint(ipAddress, Port);
    }
    
    protected IPEndPoint IpEndPoint { get; }
}