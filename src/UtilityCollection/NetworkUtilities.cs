using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.Models;

namespace FileTransfer.UtilityCollection;

internal static partial class Utilities
{
    public static ObservableCollection<User>? UsersList { get; set; }
    
    internal static User? LocalUser { get; set; }
    
    internal static async Task<IPAddress> GetIpAddressAsync()
    {
        IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
        IPAddress? finalIp;
        if ((finalIp = ipAddresses.FirstOrDefault(ip => ip.AddressFamily is AddressFamily.InterNetworkV6 or AddressFamily.InterNetwork)) != null)
            return finalIp;

        throw new InvalidIpException("No inter network IP address found.");
    }
}