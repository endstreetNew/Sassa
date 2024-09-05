using System;
using Plugin.Chat.Models;

public class UserLoginEventArgs : EventArgs
{
    public User User { get; }

    public UserLoginEventArgs(User user)
    {
        User = user;
    }
}
