using System.ComponentModel.DataAnnotations;

namespace Plugin.Chat.ViewModels;

public class Login{

[Required, MinLength(1)]
public string Username{get;set;}
}
