using System;

namespace FileTransfer.Models;

public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The received data (nullable).
    /// </summary>
    public byte[]? Buffer { get; set; }
    
    /// <summary>
    /// The total number of read bytes.
    /// </summary>
    public int Received { get; set; }
    
    /// <summary>
    /// The time the message has been received.
    /// </summary>
    public DateTime Time { get; set; }
}