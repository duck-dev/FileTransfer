using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FileTransfer.Exceptions;
using FileTransfer.Models;
using FileTransfer.ResourcesNamespace;
using FileTransfer.Services;
using FileTransfer.UtilityCollection;
using ReactiveUI;

namespace FileTransfer.ViewModels;

public class WelcomeScreenViewModel : ViewModelBase
{
    private string _username = string.Empty;
    
    private string Username 
    { 
        get => _username;
        set
        {
            _username = value;
            this.RaisePropertyChanged(nameof(IsSubmitEnabled));
        } 
    }

    private bool IsSubmitEnabled => !string.IsNullOrEmpty(Username) && !string.IsNullOrWhiteSpace(Username) && !Username.Any(char.IsWhiteSpace);

    private void Submit()
    {
        IPAddress? ownIp = null;
        Task.Run(async () =>
        {
            ownIp = await Utilities.GetLocalIpAddressAsync();
        }).Wait();
        if (ownIp is null)
            throw new InvalidIpException("Own IP is null!");
        
        ApplicationVariables.MetaData.LocalUser = new User(Utilities.EncryptID(Username, ownIp), Username, Resources.MattBlue.ToUint32(), true)
        {
            IsOnline = true
        };

        if (MainWindowViewModel.Instance is { } instance)
            instance.IsWelcomeScreenVisible = false;
        
        ApplicationVariables.MetaData.IsFirstLogin = false;
        DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
    }
}