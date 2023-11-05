using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media;
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
    
    private string _searchbarText = string.Empty;
    private ObservableCollection<User> _exposedOnlineContacts = null!;
    private ObservableCollection<User> _exposedOfflineContacts = null!;
    private User? _newContact = null;
    private MetaData _metaData = null!;

    private bool _isSearchbarVisible;
    private bool _onlineUsersExpanded;
    private bool _offlineUsersExpanded;

    private bool _wasOnlineExpanded;
    private bool _wasOfflineExpanded;
    private bool _isSearchingNewContact;
    private bool _isConnecting;
    private string? _searchbarWatermark;
    private string _oldSearchbarText = string.Empty;

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
            
            if (_metaData.UsersList is null)
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
    
    private void Initialize()
    {
        _metaData = ApplicationVariables.MetaData;
        
        if (_metaData.UsersList is null)
            return;

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
        
    }

    private void EditContact(User user)
    {
        
    }

    private void RemoveContact(User user)
    {
        
    }

    private void ToggleSearchbar(bool newContact)
    {
        IsSearchbarVisible = !IsSearchbarVisible;
        if (_metaData.UsersList is null)
            return;

        if (IsSearchbarVisible)
        {
            _wasOnlineExpanded = OnlineUsersExpanded;
            _wasOfflineExpanded = OfflineUsersExpanded;
        }
        
        OnlineUsersExpanded = IsSearchbarVisible || _wasOnlineExpanded;
        OfflineUsersExpanded = IsSearchbarVisible || _wasOfflineExpanded;
        IsSearchingNewContact = newContact;
        SearchbarText = string.Empty;

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
        NewContact = idTuple.Item1 ? idTuple.Item2 : null;
        UpdateNoElementsFoundValue();
        
        _oldSearchbarText = SearchbarText;
    }

    private void AddNewContact()
    {
        if (NewContact is null || ApplicationVariables.MetaData.UsersList is not { } usersList || usersList.Contains(NewContact))
            return;
        
        usersList.Add(NewContact);
        DataManager.SaveData(ApplicationVariables.MetaData, Utilities.MetaDataPath);
        // Change icon to checkmark
    }
}