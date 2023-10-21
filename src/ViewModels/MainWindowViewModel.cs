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
    private bool _isAccountPanelVisible;
    
    private bool _isWelcomeScreenVisible;

    internal MainWindowViewModel()
    {
        Instance = this;
        MetaDataInstance = ApplicationVariables.MetaData;
        
        if (MetaDataInstance.IsFirstLogin)
            IsWelcomeScreenVisible = true;
    }
    
    internal static WindowNotificationManager? NotificationManager { get; set; }
    internal static MainWindowViewModel? Instance { get; set; }
    
    internal bool IsWelcomeScreenVisible
    {
        get => _isWelcomeScreenVisible;
        set => this.RaiseAndSetIfChanged(ref _isWelcomeScreenVisible, value);
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

    private bool IsAccountPanelVisible
    {
        get => _isAccountPanelVisible; 
        set => this.RaiseAndSetIfChanged(ref _isAccountPanelVisible, value);
    }

    internal static void ShowNotification(Notification notification)
    {
        NotificationManager?.Show(notification);
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
        IsAccountPanelVisible = false;
    }

    internal void OpenMenu(MenuType menuType)
    {
        bool oldNotificationVisibility = IsNotificationListVisible;
        bool oldContactsVisibility = IsContactsListVisible;
        bool oldAccountVisibility = IsAccountPanelVisible;
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
                IsAccountPanelVisible = !oldAccountVisibility;
                break;
            default:
                return;
        }
    }
}