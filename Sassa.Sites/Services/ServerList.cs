using Sassa.Sites.Models;

namespace Sassa.Sites.Services
{
    public class ServerList : List<ApplicationStatus>
    {
        public ServerList()
        {
            Add(new ApplicationStatus("BRM Prod LO", "ssvsprbrphc01.sassa.local", "10.124.159.128"));
            Add(new ApplicationStatus("BRM Prod RMC", "ssvsprbrphc02.sassa.local", "10.124.159.129"));
            Add(new ApplicationStatus("BRM Qa", "ssvsqabrphc02.sassa.local", "10.124.159.131"));
        }
    }
}
