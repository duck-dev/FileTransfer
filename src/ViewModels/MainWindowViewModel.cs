using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;

namespace FileTransfer.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase
{
    // TODO: Temporary, remove when data is actually saved
    private static User? _exampleUser;
        
    
    // TODO: Temporary, remove when data is actually saved
    public MainWindowViewModel()
    {
        InitUsers();
        if (_exampleUser is null)
            throw new Exception();
        Utilities.UsersList = new List<User> { _exampleUser };
    }

    // TODO: Temporary, remove when data is actually saved
    private void InitUsers()
    {
        IPAddress? ownIp = null;
        Task.Run(async () =>
        {
            ownIp = await Utilities.GetIpAddressAsync();
        }).Wait();
        if (ownIp is null)
            throw new InvalidIpException("Own IP is null!");
        
        byte[] exampleGuid = { 240, 117, 93, 211, 216, 156, 8, 70, 131, 99, 91, 218, 42, 187, 232, 75 } ;
        _exampleUser = new User(exampleGuid, "ExampleUser", ownIp.ToString());
        Utilities.LocalUser = new User(exampleGuid, "LocalUser", ownIp.ToString());
    }
}