using System;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace FileTransfer.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase
{
    internal static WindowNotificationManager? NotificationManager { get; set; }

    internal static void ShowNotification(Notification notification)
    {
        NotificationManager?.Show(notification);
    }

    internal static void ShowNotification(string title, string message, NotificationType type, TimeSpan expiration, Action onClick, Action onClose)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var notification = new Notification(title, message, type, expiration, onClick, onClose);
            NotificationManager?.Show(notification);
        });
    }
}