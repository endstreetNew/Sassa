using Microsoft.AspNetCore.Identity;
using Sassa.eForms.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace Sassa.eForms.Services
{
    public class SassaUserStore : IUserStore<SassaUser>, IUserEmailStore<SassaUser>, IUserPasswordStore<SassaUser>, IUserClaimStore<SassaUser>, IUserPhoneNumberStore<SassaUser>
    {
        private readonly API Api;
        private SassaUser user;

        public SassaUserStore(IHttpClientFactory clientFactory)
        {
            Api = new API(clientFactory.CreateClient("UserService"));
        }

        //public async Task<SassaUser> SaveSassaUser(SassaUser item)
        //{
        //    string apicall = "api/Users";
        //    return await Api.PostRequest(apicall, item).ConfigureAwait(false);
        //}
        public async Task<SassaUser> GetSassaUser(string item)
        {
            try
            {

                string apicall = "api/Users/UserName";
                apicall = $"{apicall}/{item}";
                user = await Api.GetResult<SassaUser>(apicall).ConfigureAwait(false);
                return user;
            }
            catch
            {
                return new SassaUser();
            }

        }
        public async Task<SassaUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            //if(user == null)
            //{
            //    return new SassaUser();
            //}

            string apicall = "api/Users/Email";
            apicall = $"{apicall}/{normalizedEmail}";
            user = await Api.GetResult<SassaUser>(apicall).ConfigureAwait(false);
            return user;
        }

        public Task<string> GetEmailAsync(SassaUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);

        }

        public Task<bool> GetEmailConfirmedAsync(SassaUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetNormalizedEmailAsync(SassaUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(SassaUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(SassaUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(SassaUser user, string email, CancellationToken cancellationToken)
        {
            //string apicall = "api/Users";
            //await Api.PostRequest(apicall, user).ConfigureAwait(false);
            user.Email = email;
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(SassaUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(SassaUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(SassaUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public async Task DeletePendingVerification(SassaUser user)
        {
            string apicall = "api/Users/Email";
            apicall = $"{apicall}/{user.Email}";
            user = await Api.GetResult<SassaUser>(apicall).ConfigureAwait(false);
            if (user == null)
            {
                return;
            }
            if (!user.IsCellConfirmed)
            {
                apicall = $"api/Users/{user.Id}";
                await Api.Delete(apicall).ConfigureAwait(false);
            }
        }

        async Task<IdentityResult> IUserStore<SassaUser>.CreateAsync(SassaUser user, CancellationToken cancellationToken)
        {
            string apicall = "api/Users";
            var u = await Api.PostUser<SassaUser>(apicall, user).ConfigureAwait(false);
            user = await Api.GetResult<SassaUser>("api/Users/Email/" + user.Email);
            if (user.Id > 0)
            {
                return IdentityResult.Success;
            }
            else
            {
                //IEnumerable<IdentityError>  errs = new List<IdentityError>();
                //errs.Append(new IdentityError{Code= "2",Description = $"Could not create user."});
                //IdentityResult ir = new IdentityResult();
                //ir.Errors.Append(errs.First());
                return new IdentityResult();
            }
        }

        Task<IdentityResult> IUserStore<SassaUser>.DeleteAsync(SassaUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {

        }

        async Task<SassaUser> IUserStore<SassaUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                string apicall = "api/Users";
                apicall = $"{apicall}/{userId}";
                user = await Api.GetResult<SassaUser>(apicall).ConfigureAwait(false);
                return user;

            }
            catch
            {
                return null;
            }

        }

        async Task<SassaUser> IUserStore<SassaUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            //3
            string apicall = "api/Users/UserName";
            apicall = $"{apicall}/{normalizedUserName}";
            user = await Api.GetResult<SassaUser>(apicall).ConfigureAwait(false);
            return user;
        }

        Task<string> IUserStore<SassaUser>.GetNormalizedUserNameAsync(SassaUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IUserStore<SassaUser>.GetUserIdAsync(SassaUser user, CancellationToken cancellationToken)
        {
            //4 5 6
            return Task.FromResult(user.Id.ToString());
        }

        Task<string> IUserStore<SassaUser>.GetUserNameAsync(SassaUser user, CancellationToken cancellationToken)
        {
            //step 2
            return Task.FromResult(user.Email);
        }

        Task IUserStore<SassaUser>.SetNormalizedUserNameAsync(SassaUser user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user);
        }

        Task IUserStore<SassaUser>.SetUserNameAsync(SassaUser user, string userName, CancellationToken cancellationToken)
        {
            //string apicall = "api/Users";
            //await Api.PostRequest(apicall, user).ConfigureAwait(false);
            user.UserName = userName;
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        async Task<IdentityResult> IUserStore<SassaUser>.UpdateAsync(SassaUser user, CancellationToken cancellationToken)
        {
            string apicall = "api/Users";
            apicall = $"{apicall}/{user.Id}";
            await Api.PutRequest<SassaUser>(apicall, user).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public async Task<IList<Claim>> GetClaimsAsync(SassaUser user, CancellationToken cancellationToken)
        {
            var cl = new List<Claim>();
            await Task.Run(() => cl.Add(new Claim(ClaimTypes.Name, user.Email)));
            return cl;
        }

        public Task AddClaimsAsync(SassaUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(SassaUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(SassaUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<SassaUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberAsync(SassaUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPhoneNumberAsync(SassaUser user, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.FromResult(""); ;
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(SassaUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(SassaUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

