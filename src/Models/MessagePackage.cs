using System;
using System.ComponentModel;
using FileTransfer.Events;
using FileTransfer.Interfaces;

namespace FileTransfer.Models;

internal sealed class MessagePackage : INotifyPropertyChangedHelper
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isRead;

    internal MessagePackage(int received, DateTime time, User? sender, object[]? content = null, string? textMessage = null)
    {
        this.Content = content;
        this.TextMessage = textMessage;
        this.Received = received;
        this.Time = time;
        this.Sender = sender;
    }

    internal MessagePackage(MessageReceivedEventArgs args)
    {
        this.Content = args.Content;
        this.TextMessage = args.TextMessage;
        this.Received = args.Received;
        this.Time = args.Time;
        this.Sender = args.Sender;
    }
    
    internal object[]? Content { get; set; }
    internal string? TextMessage { get; set; }
    internal int Received { get; set; }
    internal DateTime Time { get; set; }
    internal User? Sender { get; set; }

    internal bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            NotifyPropertyChanged();
        }
    }
    
    public void NotifyPropertyChanged(string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}