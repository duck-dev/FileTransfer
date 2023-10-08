using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;
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

    private bool _isSearchbarVisible;
    private bool _onlineUsersExpanded;
    private bool _offlineUsersExpanded;

    private bool _wasOnlineExpanded;
    private bool _wasOfflineExpanded;
    private bool _isSearchingNewContact;
    private string? _searchbarWatermark;

    public ContactsListViewModel() => Initialize();
    
    private string SearchbarText
    {
        get => _searchbarText;
        set
        {
            if (_searchbarText.Equals(value))
                return;
            
            if (IsSearchingNewContact)
            {
                //NewContact = Utilities.IsIDValid(value, out User newContact) ? newContact : null; // TODO: When ID implemented
                UpdateNoElementsFoundValue();
                return;
            }
            
            if (Utilities.UsersList is null)
            {
                ExposedOnlineContacts.Clear();
                ExposedOfflineContacts.Clear();
                UpdateNoElementsFoundValue();
                return;
            }
            
            foreach (User user in Utilities.UsersList)
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
            
            this.RaiseAndSetIfChanged(ref _searchbarText, value);
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
    
    private bool NoElementsFound => IsSearchingNewContact ? NewContact is null : ExposedOnlineContacts.Count <= 0 && ExposedOfflineContacts.Count <= 0;

    private bool IsNewContactVisible => IsSearchingNewContact && !NoElementsFound;

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

    private string? SearchbarWatermark
    {
        get => _searchbarWatermark;
        set => this.RaiseAndSetIfChanged(ref _searchbarWatermark, value);
    }

    internal void UserOnlineStatusChange(User user)
    {
        if (user.IsOnline)
        {
            ExposedOfflineContacts.Remove(user);
            ExposedOnlineContacts.Add(user);
        }
        else
        {
            ExposedOnlineContacts.Remove(user);
            ExposedOfflineContacts.Add(user);
        }
    }
    
    private void Initialize()
    {
        if (Utilities.UsersList is null)
            return;
        ExposedOnlineContacts = new ObservableCollection<User>(Utilities.UsersList.Where(x => x.IsOnline));
        ExposedOfflineContacts = new ObservableCollection<User>(Utilities.UsersList.Where(x => !x.IsOnline));
        Utilities.UsersList.CollectionChanged += (sender, args) =>
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
    }

    private void ToggleSearchbar(bool newContact)
    {
        IsSearchbarVisible = !IsSearchbarVisible;
        if (Utilities.UsersList is null)
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
        UpdateNoElementsFoundValue();

        if (newContact)
        {
            ExposedOnlineContacts.Clear();
            ExposedOfflineContacts.Clear();
        }
        else
        {
            ExposedOnlineContacts = new ObservableCollection<User>(Utilities.UsersList.Where(x => x.IsOnline));
            ExposedOfflineContacts = new ObservableCollection<User>(Utilities.UsersList.Where(x => !x.IsOnline));
        }
    }

    private void UpdateNoElementsFoundValue()
    {
        this.RaisePropertyChanged(nameof(NoElementsFound));
        if(IsSearchingNewContact)
            this.RaisePropertyChanged(nameof(IsNewContactVisible));
    }
}