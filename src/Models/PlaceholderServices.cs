using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.UtilityCollection;

namespace FileTransfer.Models;

public static class PlaceholderServices
{
    // TODO: Temporary, remove when contacts are actually saved (database)
    internal static void InitUsers()
    {
        IPAddress? ownIp = null;
        Task.Run(async () =>
        {
            ownIp = await Utilities.GetIpAddressAsync();
        }).Wait();
        if (ownIp is null)
            throw new InvalidIpException("Own IP is null!");
        
        byte[] exampleGuid = { 240, 117, 93, 211, 216, 156, 8, 70, 131, 99, 91, 218, 42, 187, 232, 75 } ;
        var exampleUser = new User(exampleGuid, "ExampleUser", ownIp.ToString());
        Utilities.LocalUser = new User(exampleGuid, "LocalUser", ownIp.ToString());
        Utilities.UsersList = new ObservableCollection<User> { exampleUser };
    }
}