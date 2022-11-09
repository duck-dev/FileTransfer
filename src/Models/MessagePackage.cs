using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FileTransfer.DateFormatterUtility;
using FileTransfer.Events;
using FileTransfer.Extensions;
using FileTransfer.Interfaces;

namespace FileTransfer.Models;

internal sealed class MessagePackage : INotifyPropertyChangedHelper
{
    private enum WaitTime
    {
        OneMinute, 
        EndOfCurrentDay, 
        EndOfNextDay,
        ConstantDate
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isRead;
    private string _formattedTimeString = null!;

    // TODO: Change `files` type to actual type representing files
    internal MessagePackage(int received, DateTime time, User? sender, object[]? files = null, string? textMessage = null)
    {
        this.Files = files;
        this.TextMessage = textMessage;
        this.Received = received;
        this.Time = time;
        this.Sender = sender;

        this.TimeString = $"{time.ToShortDateString()}    {time.ToLongTimeString()}";
        UpdateTime(WaitTime.OneMinute);
    }

    internal MessagePackage(MessageReceivedEventArgs args) : this(args.Received, args.Time, args.Sender, args.Files, args.TextMessage)
    { }
    
    internal object[]? Files { get; } // TODO: Change to actual type
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
    internal string FormattedTimeString 
    { 
        get => _formattedTimeString;
        private set
        {
            _formattedTimeString = value;
            NotifyPropertyChanged();
        } 
    }
    
    public void NotifyPropertyChanged(string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void UpdateTime(WaitTime waitTimeType)
    {
        this.FormattedTimeString = DateFormatter.FormatDate(Time);

        if (waitTimeType == WaitTime.ConstantDate)
            return;
        
        TimeSpan waitTime;
        switch (waitTimeType)
        {
            case WaitTime.OneMinute:
                waitTime = TimeSpan.FromSeconds(60);
                break;
            case WaitTime.EndOfCurrentDay:
            case WaitTime.EndOfNextDay:
                DateTime tomorrow = DateTime.Now.Date.AddDays(1);
                waitTime = tomorrow.Subtract(DateTime.Now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(waitTimeType), waitTimeType, null);
        }
        
        Task.Delay(waitTime).ContinueWith(x => { UpdateTime(waitTimeType.Next()); });
    }
}