using System.Net;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.Models;
using FileTransfer.ResourcesNamespace;
using FileTransfer.Services;
using FileTransfer.UtilityCollection;

namespace FileTransfer.ViewModels;

public class WelcomeScreenViewModel : ViewModelBase
{
    private string Username { get; set; } = string.Empty;

    private void Submit()
    {
        IPAddress? ownIp = null;
        Task.Run(async () =>
        {
            ownIp = await Utilities.GetIpAddressAsync();
        }).Wait();
        if (ownIp is null)
            throw new InvalidIpException("Own IP is null!");
        
        ApplicationVariables.MetaData.LocalUser = new User(Utilities.EncryptID(Username, ownIp), Username, Resources.MattBlue.ToUint32())
        {
            IsOnline = true
        };

        if (MainWindowViewModel.Instance is { } instance)
            instance.IsWelcomeScreenVisible = false;
        
        ApplicationVariables.MetaData.IsFirstLogin = false;
        DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
    }
}