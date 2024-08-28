using Sassa.Brm.Portal.Models;

namespace Sassa.Brm.Portal.Services
{
    public class MenuItems:List<MenuItem>
    {
        public  MenuItems()
        {
            Add(new MenuItem(1, "Local Office", "Http://ssvsprbrphc01.sassa.local", "images/sassaicon.png"));
            Add(new MenuItem(2, "RMC Office", "Http://ssvsprbrphc02.sassa.local:81", "images/sassaicon.png"));
            Add(new MenuItem(3, "Reports", "Http://ssvsqabrshc02.sassa.local:81", "images/sassaicon.png"));
            Add(new MenuItem(4, "The Document Warehouse", "https://rsweb.tdw.co.za/oneilOrder/", "images/sassaicon.png"));
            Add(new MenuItem(5, "Kofax scan", "http://10.124.154.88/sassaongoing/ValidationLogin.aspx", "images/sassaicon.png"));
            Add(new MenuItem(6, "Brm QA", "Http://ssvsqabrshc02.sassa.local", "images/sassaicon.png"));
        }
    }
}
