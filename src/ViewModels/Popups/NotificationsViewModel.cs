using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using FileTransfer.Models;
using ReactiveUI;

namespace FileTransfer.ViewModels;

internal class NotificationsViewModel : ViewModelBase
{
    private bool _unreadNotificationsAvailable;
    private int _unreadNotificationsCount;
    
    public NotificationsViewModel()
    {
        Instance = this;
        if (MainWindowViewModel.Instance is { } mainWindowViewModel)
            mainWindowViewModel.NotificationsViewModelInstance = this;
        Notifications.CollectionChanged += (sender, args) => this.RaisePropertyChanged(nameof(NotificationsAvailable));
    }
    
    internal static NotificationsViewModel? Instance { get; private set; }
    
    private ObservableCollection<NotificationItem> Notifications { get; } = new();

    private bool NotificationsAvailable => Notifications.Count > 0;

    internal bool UnreadNotificationsAvailable
    {
        get => _unreadNotificationsAvailable;
        set => this.RaiseAndSetIfChanged(ref _unreadNotificationsAvailable, value);
    }
    
    internal string UnreadNotificationsCountStr => UnreadNotificationsCount.ToString();

    private int UnreadNotificationsCount
    {
        get => _unreadNotificationsCount;
        set
        {
            _unreadNotificationsCount = value;
            this.RaisePropertyChanged(nameof(UnreadNotificationsCountStr));
        }
    }

    internal void AddNotification(Notification notificationCard)
    {
        var notificationItem = new NotificationItem(notificationCard);
        Notifications.Insert(0, notificationItem);
        
        UnreadNotificationsAvailable = true;
        UnreadNotificationsCount++;
    }

    internal void RemoveNotification(NotificationItem item)
    {
        if (!item.IsRead)
            UnreadNotificationsCount--;
        if (UnreadNotificationsCount <= 0)
            UnreadNotificationsAvailable = false;
        
        Notifications.Remove(item);
    }

    internal void MarkAllAsRead()
    {
        foreach (var item in Notifications)
            item.IsRead = true;
        
        UnreadNotificationsAvailable = false;
        UnreadNotificationsCount = 0;
    }
}