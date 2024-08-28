using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Sassa.eForms.Internal.Extentions
{
    public static class Security
    {
        public static List<string> GetRoles(this ClaimsPrincipal User)
        {
            var roles = new List<string>();

            var wi = (WindowsIdentity)User.Identity;
            if (wi.Groups != null)
            {
                foreach (var group in wi.Groups)
                {
                    if (wi.Groups != null)
                    {
                        try
                        {
                            string role = group.Translate(typeof(NTAccount)).ToString();
                            if (!role.Contains("SASSA")) continue;
                            if (role.Contains("\\"))
                            {
                                role = role.Substring(role.LastIndexOf("\\") + 1);
                            }
                            if (!role.Contains("GRP_LO_")) continue;
                            roles.Add(role);
                        }
                        catch// (Exception e)
                        {
                            // ignored
                        }

                    }
                }
            }
            return roles;
        }
    }
}
