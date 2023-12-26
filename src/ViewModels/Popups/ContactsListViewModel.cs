using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FileTransfer.Enums;
using FileTransfer.Models;
using FileTransfer.ResourcesNamespace;
using FileTransfer.Services;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels.Dialogs;
using ReactiveUI;

namespace FileTransfer.ViewModels;

internal class ContactsListViewModel : ViewModelBase
{
    private const string SearchNewContactWatermark = "Enter an ID to find a new contact...";
    private const string SearchExistingContactWatermark = "Find a contact...";
    private const string AddContactIconPath = $"{Utilities.AssetsPath}Add-Contact-Green.png";
    private const string CheckmarkIconPath = $"{Utilities.AssetsPath}Checkmark-Green.png";
    
    private string _searchbarText = string.Empty;
    private ObservableCollection<User> _exposedOnlineContacts = null!;
    private ObservableCollection<User> _exposedOfflineContacts = null!;
    private User? _newContact = null;
    private MetaData _metaData = null!;
    private ReceiveViewModel? _receiveViewModel;

    private bool _isSearchbarVisible;
    private bool _onlineUsersExpanded;
    private bool _offlineUsersExpanded;
    private bool _wasOnlineExpanded;
    private bool _wasOfflineExpanded;
    private bool _isSearchingNewContact;
    private bool _isConnecting;
    private bool _addContactButtonEnabled;
    private string? _searchbarWatermark;
    private string _oldSearchbarText = string.Empty;

    private Bitmap? _addContactButtonIcon = null!;
    private Bitmap? _addContactIcon;
    private Bitmap? _checkmarkIcon;

    public ContactsListViewModel() => Initialize();
    
    internal ReactiveCommand<User, Unit> WriteMessageCommand { get; private set; } = null!;
    internal ReactiveCommand<User, Unit> EditContactCommand { get; private set; } = null!;
    internal ReactiveCommand<User, Unit> RemoveContactCommand { get; private set; } = null!;

    private string SearchbarText
    {
        get => _searchbarText;
        set
        {
            if (_searchbarText.Equals(value))
                return;
            
            this.RaiseAndSetIfChanged(ref _searchbarText, value);

            if (IsSearchingNewContact)
                return;
            
            if (_metaData.UsersList.Count <= 0)
            {
                ExposedOnlineContacts.Clear();
                ExposedOfflineContacts.Clear();
                UpdateNoElementsFoundValue();
                return;
            }
            
            foreach (User user in _metaData.UsersList)
            {
                var contactsList = user.IsOnline ? ExposedOnlineContacts : ExposedOfflineContacts;
                if (string.IsNullOrEmpty(value) || user.Nickname.Contains(value, StringComparison.OrdinalIgnoreCase) || user.Username.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    if (contactsList.Contains(user))
                        continue;
                        
                    if (contactsList.Count <= 0)
                    {
                        contactsList.Add(user);
                        continue;
                    }
                        
                    for (int i = 0; i < contactsList.Count; i++)
                    {
                        if (i == 0 && Utilities.CompareStringsAlphabetically(user.Nickname, contactsList[i].Nickname) <= 0)
                        {
                            contactsList.Insert(i, user);
                            break;
                        }
                            
                        if (i < contactsList.Count - 1 && (Utilities.CompareStringsAlphabetically(user.Nickname, contactsList[i].Nickname) <= 0 || Utilities.CompareStringsAlphabetically(user.Nickname, contactsList[i + 1].Nickname) <= 0)) 
                            continue;
                        contactsList.Insert(i + 1, user);
                        break;
                    }
                }
                else
                {
                    contactsList.Remove(user);
                }
            }
            
            UpdateNoElementsFoundValue();
        }
    }
    
    private ObservableCollection<User> ExposedOnlineContacts 
    {
        get => _exposedOnlineContacts;
        set
        {
            this.RaiseAndSetIfChanged(ref _exposedOnlineContacts, value);
            UpdateNoElementsFoundValue();
            this.RaisePropertyChanged(nameof(UsersOnlineCount));
            ExposedOnlineContacts.CollectionChanged += (sender, args) => this.RaisePropertyChanged(nameof(UsersOnlineCount));
        }
    }

    private ObservableCollection<User> ExposedOfflineContacts
    {
        get => _exposedOfflineContacts;
        set
        {
            this.RaiseAndSetIfChanged(ref _exposedOfflineContacts, value);
            UpdateNoElementsFoundValue();
            this.RaisePropertyChanged(nameof(UsersOfflineCount));
            ExposedOfflineContacts.CollectionChanged += (sender, args) => this.RaisePropertyChanged(nameof(UsersOfflineCount));
        }
    }

    private User? NewContact
    {
        get => _newContact;
        set => this.RaiseAndSetIfChanged(ref _newContact, value);
    }
    
    private bool NoElementsFound => IsSearchingNewContact ? NewContact is null && !IsConnecting : ExposedOnlineContacts.Count <= 0 && ExposedOfflineContacts.Count <= 0;

    private bool IsNewContactVisible => IsSearchingNewContact && !NoElementsFound && !IsConnecting;

    private int UsersOnlineCount => ExposedOnlineContacts.Count;
    private int UsersOfflineCount => ExposedOfflineContacts.Count;

    private bool OnlineUsersExpanded
    {
        get => _onlineUsersExpanded;
        set => this.RaiseAndSetIfChanged(ref _onlineUsersExpanded, value);
    }
    private bool OfflineUsersExpanded
    {
        get => _offlineUsersExpanded;
        set => this.RaiseAndSetIfChanged(ref _offlineUsersExpanded, value);
    }

    private bool IsSearchbarVisible
    {
        get => _isSearchbarVisible; 
        set => this.RaiseAndSetIfChanged(ref _isSearchbarVisible, value);
    }

    private bool IsSearchingNewContact
    {
        get => _isSearchingNewContact;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSearchingNewContact, value);
            SearchbarWatermark = value == true ? SearchNewContactWatermark : SearchExistingContactWatermark;
            this.RaisePropertyChanged(nameof(IsNewContactVisible));
        }
    }

    private bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            this.RaiseAndSetIfChanged(ref _isConnecting, value);
            UpdateNoElementsFoundValue();
        }
    }

    private string? SearchbarWatermark
    {
        get => _searchbarWatermark;
        set => this.RaiseAndSetIfChanged(ref _searchbarWatermark, value);
    }

    private bool IsAddContactButtonEnabled
    {
        get => _addContactButtonEnabled;
        set => this.RaiseAndSetIfChanged(ref _addContactButtonEnabled, value);
    }

    private Bitmap? AddContactButtonIcon
    {
        get => _addContactButtonIcon;
        set => this.RaiseAndSetIfChanged(ref _addContactButtonIcon, value);
    }

    internal void UserOnlineStatusChange(object? sender, User user)
    {
        if (user.IsOnline)
        {
            if(ExposedOfflineContacts.Contains(user))
                ExposedOfflineContacts.Remove(user);
            if(!ExposedOnlineContacts.Contains(user))
                ExposedOnlineContacts.Add(user);
        }
        else
        {
            if(ExposedOnlineContacts.Contains(user))
                ExposedOnlineContacts.Remove(user);
            if(!ExposedOfflineContacts.Contains(user))
                ExposedOfflineContacts.Add(user);
        }
    }

    internal void ClearData()
    {
        if (!IsSearchingNewContact)
            return;

        ToggleSearchbar(false);
    }
    
    private void Initialize()
    {
        _metaData = ApplicationVariables.MetaData!;
        _receiveViewModel = ReceiveViewModel.Instance;
        
        _addContactIcon = Utilities.CreateImage(AddContactIconPath);
        _checkmarkIcon = Utilities.CreateImage(CheckmarkIconPath);
        AddContactButtonIcon = _addContactIcon;

        List<Task> tasks = new List<Task>();
        foreach (User user in _metaData.UsersList)
            tasks.Add(Task.Run(async () => await user.CheckUserOnline()));
        Task.Run(async () => await Task.WhenAll(tasks)).Wait();
        
        ExposedOnlineContacts = new ObservableCollection<User>(_metaData.UsersList.Where(x => x.IsOnline));
        ExposedOfflineContacts = new ObservableCollection<User>(_metaData.UsersList.Where(x => !x.IsOnline));
        _metaData.UsersList.CollectionChanged += (sender, args) =>
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add when args.NewItems != null:
                    foreach (var item in args.NewItems)
                    {
                        if(item is User user)
                            (user.IsOnline ? ExposedOnlineContacts : ExposedOfflineContacts).Add(user);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove when args.OldItems != null:
                    foreach (var item in args.OldItems)
                    {
                        if(item is User user)
                            (user.IsOnline ? ExposedOnlineContacts : ExposedOfflineContacts).Remove(user);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    break;
            }

            UpdateNoElementsFoundValue();
        };

        Utilities.OnUserOnlineStatusChanged += UserOnlineStatusChange;

        WriteMessageCommand = ReactiveCommand.Create<User>(WriteMessage);
        EditContactCommand = ReactiveCommand.Create<User>(EditContact);
        RemoveContactCommand = ReactiveCommand.Create<User>(RemoveContact);
    }

    private void WriteMessage(User user)
    {
        if (SendViewModel.Instance is { } sendViewModel)
            sendViewModel.SetReceiver(user);
    }

    private void EditContact(User user)
    {
        foreach(User otherUser in ExposedOnlineContacts)
            otherUser.DiscardChanges();
        foreach(User otherUser in ExposedOfflineContacts)
            otherUser.DiscardChanges();
        
        user.IsEditingContact = true;
    }

    private void RemoveContact(User user)
    {
        if (!_metaData.UsersList.Contains(user) || _receiveViewModel is null)
            return;
        
        string dialogTitle = $"Are you sure you want to remove '{user.Nickname}' from your contact list?";
        Action action = () =>
        {
            _metaData.UsersList.Remove(user);
            DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
        };
        _receiveViewModel.CurrentDialog = new ConfirmationDialogViewModel(_receiveViewModel, dialogTitle,
            new[] { Resources.MainRed, Resources.MainGrey },
            new[] { Colors.White, Colors.White },
            new[] { $"Yes, remove '{user.Nickname}'!", "Cancel" },
            action);
    }

    private void ToggleSearchbar(bool newContact)
    {
        IsSearchbarVisible = !IsSearchbarVisible;

        if (IsSearchbarVisible)
        {
            _wasOnlineExpanded = OnlineUsersExpanded;
            _wasOfflineExpanded = OfflineUsersExpanded;
        }
        
        OnlineUsersExpanded = IsSearchbarVisible || _wasOnlineExpanded;
        OfflineUsersExpanded = IsSearchbarVisible || _wasOfflineExpanded;
        IsSearchingNewContact = newContact;
        SearchbarText = string.Empty;
        _oldSearchbarText = SearchbarText;
        NewContact = null;
        AddContactButtonIcon = _addContactIcon!;

        if (newContact)
        {
            ExposedOnlineContacts.Clear();
            ExposedOfflineContacts.Clear();
            NewContact = null;
        }
        else
        {
            ExposedOnlineContacts = new ObservableCollection<User>(_metaData.UsersList.Where(x => x.IsOnline));
            ExposedOfflineContacts = new ObservableCollection<User>(_metaData.UsersList.Where(x => !x.IsOnline));
        }
        
        UpdateNoElementsFoundValue();
    }

    private void UpdateNoElementsFoundValue()
    {
        this.RaisePropertyChanged(nameof(NoElementsFound));
        if(IsSearchingNewContact)
            this.RaisePropertyChanged(nameof(IsNewContactVisible));
    }

    private async Task SearchNewContact()
    {
        if (!IsSearchingNewContact || _oldSearchbarText.Equals(SearchbarText))
            return;
        
        IsConnecting = true;
        await Task.Delay(100);
        Tuple<bool, User?> idTuple = await Utilities.IsIDValid(SearchbarText);
        IsConnecting = false;
        
        User? newContact = idTuple.Item1 ? idTuple.Item2 : null;
        if (newContact is not null && _metaData.UsersList.Contains(newContact))
        {
            uint colorCode = _metaData.UsersList.First(x => newContact.Equals(x)).ColorCode;
            Color color = Color.FromUInt32(colorCode);
            newContact.ColorBrush = new SolidColorBrush(color);
        }
        NewContact = newContact;
        
        UpdateNoElementsFoundValue();
        CheckAddingContact();
        
        _oldSearchbarText = SearchbarText;
    }

    private async Task AddNewContact()
    {
        if (NewContact is null || _metaData.UsersList.Contains(NewContact))
            return;
        
        _metaData.UsersList.Add(NewContact);
        DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
        CheckAddingContact();
        
        IPEndPoint endPoint = new IPEndPoint(NewContact.IP!, Utilities.ContactCommunicationPort);
        using Socket client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        bool establishedConnection = await Utilities.EstablishConnection(endPoint, client);
        if (!establishedConnection)
        {
            Utilities.CloseConnection(client);
            return;
        }

        await Utilities.SendCommunicationCode(CommunicationCode.AddedAsContact, client);
    }

    private void CheckAddingContact()
    {
        if (NewContact is null)
            return;
        
        bool canAddContact = !_metaData.UsersList.Contains(NewContact);
        IsAddContactButtonEnabled = canAddContact;
        AddContactButtonIcon = canAddContact ? _addContactIcon : _checkmarkIcon;
    }
}