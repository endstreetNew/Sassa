using System.ComponentModel.DataAnnotations;

namespace Plugin.Chat.ViewModels;
public class PostMessage
{
    [Required, MinLength(1)]
    public string Text { get; set; }
}
