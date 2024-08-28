using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;


public class WindowsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public WindowsAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _httpContextAccessor.HttpContext!.User.Identity;

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity!)));
        //return Task.FromResult(new AuthenticationState(new WindowsIdentity(_httpContextAccessor.HttpContext!.User.Identity.Name)));

    }
}

