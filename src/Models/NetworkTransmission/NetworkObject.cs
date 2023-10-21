using System.Net;

namespace FileTransfer.Models.NetworkTransmission;

internal abstract class NetworkObject
{
    protected const int BufferSize = 1024;
    private const int Port = 31415;
    
    protected NetworkObject(IPAddress? ipAddress = null)
    {
        ipAddress ??= IPAddress.Any;
        IpEndPoint = new IPEndPoint(ipAddress, Port);
        MetaDataInstance = ApplicationVariables.MetaData;
    }
    
    protected IPEndPoint IpEndPoint { get; }
    
    protected MetaData MetaDataInstance { get; }
}