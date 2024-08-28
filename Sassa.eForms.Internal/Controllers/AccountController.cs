using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sassa.eForms.Models;
using System.Threading.Tasks;

namespace Sassa.eForms.Accounts.Controllers
{
    public class SassaAccountController : Controller
    {
        private readonly UserManager<SassaUser> _userManager;
        private readonly SignInManager<SassaUser> _signInManager;
        private readonly IDataProtector _dataProtector;
        private AuthenticationStateProvider _state;

        public SassaAccountController(IDataProtectionProvider dataProtectionProvider, UserManager<SassaUser> userManager, SignInManager<SassaUser> signInManager, AuthenticationStateProvider state)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("SignIn");
            _userManager = userManager;
            _signInManager = signInManager;
            _state = state;
        }

        [HttpGet("sassaaccount/signinactual")]
        public async Task<IActionResult> SignInActual(string t)
        {
            var data = _dataProtector.Unprotect(t);

            var parts = data.Split('|');

            SassaUser user = await _userManager.FindByIdAsync(parts[0]);

            var isTokenValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "SignIn", parts[1]);

            if (isTokenValid)
            {
                await _signInManager.SignInAsync(user, true);
                //var astate = _state.GetAuthenticationStateAsync();
                if (parts.Length == 3 && Url.IsLocalUrl(parts[2]))
                {
                    return Redirect(parts[2]);
                }
                return Redirect($"{this.Request.PathBase}/");
            }
            else
            {
                return Unauthorized("STOP!");
            }
        }

        [HttpGet("sassaaccount/logout")]
        public async Task<IActionResult> SignOutActual()
        {
            await _signInManager.SignOutAsync();

            return Redirect($"{this.Request.PathBase}/");
        }
    }
}
