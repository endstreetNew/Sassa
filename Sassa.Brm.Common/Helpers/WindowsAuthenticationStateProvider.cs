using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


public class WindowsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public WindowsAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }
    //public override Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    ClaimsPrincipal principal = new ClaimsPrincipal();
    //    if (_httpContextAccessor is not null)
    //    {
    //        var identity = _httpContextAccessor.HttpContext!.User.Identity;
    //        principal = new ClaimsPrincipal(identity!);
    //    }

    //    return Task.FromResult(new AuthenticationState(principal));
    //}

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor?.HttpContext?.User ?? new ClaimsPrincipal(); 
        //var principal = user ?? new ClaimsPrincipal();
        return Task.FromResult(new AuthenticationState(user));
    }

}

