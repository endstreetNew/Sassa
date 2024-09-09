using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using System.Security.Claims;

namespace Sassa.Brm.Common.Services;

public class SessionService(StaticService _staticservice,AuthenticationStateProvider auth, IJSRuntime jsRuntime)
{

    private UserSession _session = new UserSession("", "", "", ""); 
    public UserSession session {
        get
        {
            if (!_session.IsLoggedIn())
            {
                try { GetUserSession().Wait(); }
                catch {}
            }
            return _session;
        } 
    }
    public event EventHandler? UserOfficeChanged;

    public async Task GetUserSession()
    {
        ClaimsPrincipal claimsPrincipal = (await auth.GetAuthenticationStateAsync()).User;
        //Get user details
        _session = claimsPrincipal.GetSession();
        //Get user region and office link
        UpdateUserOffice();
    }

    public void UpdateUserOffice()
    {
        while (!_staticservice.IsInitialized)
        {
            //wait here on startup
        }
        _session.Office = _staticservice.GetUserLocalOffice(_session.SamName);
        //Trigger the change for UI update
        UserOfficeChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task LogToConsole(string message)
    {
        await jsRuntime.InvokeVoidAsync("console.log", message);
    }

}
