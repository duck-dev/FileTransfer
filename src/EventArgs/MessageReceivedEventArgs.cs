using System;
using FileTransfer.Models;

namespace FileTransfer.Events;

public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The received files (nullable).
    /// </summary>
    internal FileObject[]? Files { get; set; }
    
    /// <summary>
    /// Optional text message (string).
    /// </summary>
    internal string? TextMessage { get; set; }

    /// <summary>
    /// The time the message has been received.
    /// </summary>
    internal DateTime Time { get; set; }
    
    /// <summary>
    /// The sender as a <see cref="User"/> instance.
    /// </summary>
    internal User? Sender { get; set; }
}