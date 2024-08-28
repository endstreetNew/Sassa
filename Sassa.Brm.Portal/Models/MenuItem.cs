namespace Sassa.Brm.Portal.Models
{
    public class MenuItem
    {
        public MenuItem(int position,string name, string url,string icon) {
            Position = position;
            Name = name;
            Url = url;
            Icon = icon;
        }
        public int Position { get; set; }
        public string Name { get; set; }
        public string Url { get; set; } 
        public string Icon { get; set; }    

    }
}
