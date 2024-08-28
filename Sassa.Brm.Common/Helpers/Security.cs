using Sassa.Brm.Common.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Sassa.Brm.Common.Helpers
{
    public static class Security
    {
        public static List<string> GetRoles(this WindowsIdentity user)
        {
            List<string> result = new List<string>();
            var groups = user.Claims.Where(q => q.Type == ClaimTypes.GroupSid).Select(q => q.Value).ToList();
            foreach (var role in groups)
            {
                try
                {
                    var group = new System.Security.Principal.SecurityIdentifier(role).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                    if (!group.Contains("SASSA")) continue;

                    result.Add(group.Substring(6));
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }

        public static List<string> GetRoles(this ClaimsPrincipal user)
        {
            List<string> result = new List<string>();
            var groups = user.Claims.Where(q => q.Type == ClaimTypes.GroupSid).Select(q => q.Value).ToList();
            foreach (var role in groups)
            {
                try
                {
                    var group = new System.Security.Principal.SecurityIdentifier(role).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                    if (!group.Contains("SASSA")) continue;

                    result.Add(group.Substring(6));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }

        public static UserSession GetSession(this ClaimsPrincipal cp)
        {
            //Get user details
            string? userName = cp.Identity!.Name!.TrimStart(@"SASSA\\".ToCharArray());
            if (string.IsNullOrEmpty(userName)) throw new Exception("Authentication failed.");
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);
            UserSession _session = new UserSession(user.Name, user.Surname, user.SamAccountName, user.EmailAddress);

            //Get user Roles
            List<string> result = new List<string>();
            var groups = cp.Claims.Where(q => q.Type == ClaimTypes.GroupSid).Select(q => q.Value).ToList();
            foreach (var role in groups)
            {
                try
                {
                    var group = new System.Security.Principal.SecurityIdentifier(role).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                    if (!group.Contains("SASSA")) continue;

                    result.Add(group.Substring(6));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            _session.Roles = result;
           
            return _session;
        }

        public static string? GetADEmail(this string username)
        {
            try
            {
                var di = new DirectoryEntry($"LDAP://DC=SASSA,DC=local");
                string[] loadProps = new string[] { "name", "mail" };
                DirectorySearcher ds = new DirectorySearcher(di, $"SamAccountName={username}", loadProps);
                SearchResult sr = ds.FindOne()!;
                DirectoryEntry entry = sr.GetDirectoryEntry();
                return (string)entry.Properties["mail"].Value!;
            }
            catch
            {
                //throw new Exception("Could not retrieve AD Email"); can't throw here because of bulk operations
                return null;
            }
        }
    }
}
