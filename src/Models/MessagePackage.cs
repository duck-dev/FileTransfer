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

        this.TimeString = $"{time.ToShortDateString()}    {time.ToLongTimeString()}";
    }

    internal MessagePackage(MessageReceivedEventArgs args)
    {
        this.Content = args.Content;
        this.TextMessage = args.TextMessage;
        this.Received = args.Received;
        this.Time = args.Time;
        this.Sender = args.Sender;
        
        this.TimeString = $"{args.Time.ToShortDateString()}    {args.Time.ToLongTimeString()}";
    }
    
    internal object[]? Content { get; }
    internal string? TextMessage { get; }
    internal int Received { get; }
    internal DateTime Time { get; }
    internal User? Sender { get; }

    internal bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            NotifyPropertyChanged();
        }
    }
    
    internal string TimeString { get; }
    
    public void NotifyPropertyChanged(string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}