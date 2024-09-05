using Plugin.Chat.ViewModels;
namespace Plugin.Chat.Services;
public class AutoLoginService(IChatService ChatService, IConnectedClientService connectedClientService)
{
    public User? User { get; set; }
    Login _login = new();
    bool _processing = false;

    public void Login(string user)
    {
        _processing = true;
        while (connectedClientService.Client == null)
        {
            Task.Delay(1000);
        }
        User = ChatService.Login(_login.Username, connectedClientService.Client);
    }
    public void Login(User user)
    {
        _processing = true;
        while (connectedClientService.Client == null)
        {
            Task.Delay(1000);
        }
        User = ChatService.Login(user, connectedClientService.Client);
    }
}