using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FileTransfer.Exceptions;

namespace FileTransfer.UtilityCollection;

public static partial class Utilities
{
    public static async Task<IPAddress> GetIpAddress()
    {
        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
        IPAddress? finalIp;
        if ((finalIp = ipAddresses.FirstOrDefault(ip => ip.AddressFamily is AddressFamily.InterNetworkV6 or AddressFamily.InterNetwork)) != null)
            return finalIp;

        throw new InvalidIpException("No inter network IP address found.");
    }
}