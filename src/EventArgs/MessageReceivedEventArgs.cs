using System;
using FileTransfer.Models;

namespace FileTransfer.Events;

public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The received data (nullable).
    /// </summary>
    public object[]? Content { get; set; }
    
    /// <summary>
    /// Optional text message (string).
    /// </summary>
    public string? TextMessage { get; set; } 
    
    /// <summary>
    /// The total number of read bytes.
    /// </summary>
    public int Received { get; set; }
    
    /// <summary>
    /// The time the message has been received.
    /// </summary>
    public DateTime Time { get; set; }
    
    /// <summary>
    /// The sender as a <see cref="User"/> instance.
    /// </summary>
    public User? Sender { get; set; }
    
    /// <summary>
    /// Saves whether the message has been read or not.
    /// </summary>
    public bool IsRead { get; set; }
}