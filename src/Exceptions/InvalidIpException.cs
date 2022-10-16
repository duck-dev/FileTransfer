using System;

namespace FileTransfer.Exceptions;

[Serializable]
public class InvalidIpException : Exception
{
    public InvalidIpException() : base() { }
    public InvalidIpException(string message) : base(message) { }
    public InvalidIpException(string message, Exception inner) : base(message, inner) { }
}