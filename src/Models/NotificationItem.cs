using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using FileTransfer.DateFormatterUtility;
using FileTransfer.Enums;
using FileTransfer.Interfaces;
using FileTransfer.ResourcesNamespace;

namespace FileTransfer.Models;

internal class NotificationItem : INotifyPropertyChangedHelper, IFormattableTime
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isRead;
    private string _formattedTimeString = string.Empty;
    
    internal NotificationItem(Notification notificationInstance)
    {
        NotificationInstance = notificationInstance;
        Color = notificationInstance.Type switch
        {
            NotificationType.Information => Resources.MainBlueBrush,
            NotificationType.Success => Resources.MainGreenBrush,
            NotificationType.Warning => Resources.MainYellowBrush,
            NotificationType.Error => Resources.MainRedBrush,
            _ => Resources.WhiteBrush
        };
        
        DateFormatter.UpdateTime(WaitTime.OneMinute, this, DateTime.Now);
    }
    
    public string FormattedTimeString 
    { 
        get => _formattedTimeString;
        set
        {
            _formattedTimeString = value;
            NotifyPropertyChanged();
        } 
    }
    
    internal Notification NotificationInstance { get; }
    internal SolidColorBrush Color { get; }
    internal bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            NotifyPropertyChanged();
        }
    }
    
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}