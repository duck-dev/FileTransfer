namespace FileTransfer.Enums;

public enum CommunicationCode : byte
{
    CheckUsername = 0,
    UsernameChanged = 1,
    IPChanged = 2,
    UpdateOnlineStatus = 3,
    AddedAsContact = 4
}