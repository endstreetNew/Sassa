using Sassa.Brm.Common.Models;
using System.Security.Principal;

namespace Sassa.Brm.Common.Services
{
    public interface ISessionService
    {
        UserSession? session { get; set; }

        event EventHandler? SessionInitialized;
        UserSession? GetUserSession(WindowsIdentity identity);
    }
}