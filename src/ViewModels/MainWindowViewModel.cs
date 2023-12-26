using System;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using FileTransfer.Enums;
using FileTransfer.Models;
using ReactiveUI;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace FileTransfer.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase
{
    private bool _isNotificationListVisible;
    private bool _isContactsListVisible;
    private bool _isProfilePageVisible;

    private bool _isMaximizedMessageVisible;
    private bool _isWelcomeScreenVisible;

    private MessagePackage? _maximizedMessage;
    private NotificationsViewModel? _notificationsViewModelInstance;

    internal MainWindowViewModel()
    {
        Instance = this;
        MetaDataInstance = ApplicationVariables.MetaData!;
        
        if (MetaDataInstance.IsFirstLogin)
            IsWelcomeScreenVisible = true;

        if (NotificationsViewModel.Instance is { } notificationsViewModel)
            NotificationsViewModelInstance = notificationsViewModel;
    }
    
    internal static WindowNotificationManager? NotificationManager { get; set; }
    internal static MainWindowViewModel? Instance { get; set; }
    
    internal bool IsWelcomeScreenVisible
    {
        get => _isWelcomeScreenVisible;
        set => this.RaiseAndSetIfChanged(ref _isWelcomeScreenVisible, value);
    }

    internal NotificationsViewModel? NotificationsViewModelInstance
    {
        get => _notificationsViewModelInstance; 
        set => this.RaiseAndSetIfChanged(ref _notificationsViewModelInstance, value);
    }

    private MetaData MetaDataInstance { get; }

    private bool IsNotificationListVisible
    {
        get => _isNotificationListVisible; 
        set => this.RaiseAndSetIfChanged(ref _isNotificationListVisible, value);
    }
    
    private bool IsContactsListVisible
    {
        get => _isContactsListVisible; 
        set => this.RaiseAndSetIfChanged(ref _isContactsListVisible, value);
    }

    private bool IsProfilePageVisible
    {
        get => _isProfilePageVisible; 
        set => this.RaiseAndSetIfChanged(ref _isProfilePageVisible, value);
    }

    private bool IsMaximizedMessageVisible
    {
        get => _isMaximizedMessageVisible;
        set => this.RaiseAndSetIfChanged(ref _isMaximizedMessageVisible, value);
    }

    private MessagePackage? MaximizedMessage
    {
        get => _maximizedMessage;
        set => this.RaiseAndSetIfChanged(ref _maximizedMessage, value);
    }

    internal static void ShowNotification(Notification notification)
    {
        NotificationManager?.Show(notification);
        if(NotificationsViewModel.Instance is { } notificationsViewModel)
            notificationsViewModel.AddNotification(notification);
    }

    internal static void ShowNotification(string title, string message, NotificationType type, TimeSpan expiration, 
        Action? onClick = null, Action? onClose = null)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var notification = new Notification(title, message, type, expiration, onClick, onClose);
            ShowNotification(notification);
        });
    }
    
    internal void CloseMenus()
    {
        IsNotificationListVisible = false;
        IsContactsListVisible = false;
        IsProfilePageVisible = false;
    }

    internal void OpenMenu(MenuType menuType)
    {
        bool oldNotificationVisibility = IsNotificationListVisible;
        bool oldContactsVisibility = IsContactsListVisible;
        bool oldAccountVisibility = IsProfilePageVisible;
        CloseMenus();

        switch (menuType)
        {
            case MenuType.Notifications:
                IsNotificationListVisible = !oldNotificationVisibility;
                break;
            case MenuType.Contacts:
                IsContactsListVisible = !oldContactsVisibility;
                break;
            case MenuType.Account:
                IsProfilePageVisible = !oldAccountVisibility;
                break;
            default:
                return;
        }
    }

    internal void ToggleMaximizedMessage(MessagePackage? message)
    {
        MaximizedMessage = message;
        IsMaximizedMessageVisible = message is not null;
    }
}