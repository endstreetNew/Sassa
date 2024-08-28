using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Sassa.eForms.Models
{
    public class UserSession
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string SamName { get; set; }
        public List<string> Roles { get; set; }

        public UserSession()
        {

        }

        public bool IsAuththorized()
        {
            return Roles.Any();
        }
        //private void SetUserGroups()
        //{
        //    var wi = WindowsIdentity.GetCurrent();

        //    if (wi.Groups != null)
        //    {
        //        foreach (var group in wi.Groups)
        //        {
        //            try
        //            {
        //                string role = group.Translate(typeof(NTAccount)).ToString();
        //                if (!role.Contains("SASSA")) continue;
        //                if (role.Contains("\\"))
        //                {
        //                    role = role.Substring(role.LastIndexOf("\\") + 1);
        //                }
        //                Roles.Add(role);
        //            }
        //            catch (Exception e)
        //            {
        //                // ignored
        //            }
        //        }
        //    }
        //}
    }
}
