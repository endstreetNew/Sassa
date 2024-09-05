using System;
using Plugin.Chat.Models;
public class UserLogoutEventArgs : EventArgs
{
    public string Username { get; }

    public UserLogoutEventArgs(string username)
    {
        Username = username;
    }
}

public class MessageEventArgs : EventArgs
{
    public string Username { get; }
    public string Message { get; }
}
