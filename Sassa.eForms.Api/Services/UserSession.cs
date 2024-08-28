using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Sassa.eForms.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sassa.eForms.Services
{
    public interface IUserSession
    {
        SassaUser User { get; set; }
        string IpAddress { get; set; }
        void Invalidate();
    }
    public class UserSession : IUserSession
    {
        public SassaUser User { get; set; }
        public string IpAddress { get; set; }

        private IHttpContextAccessor _accessor;

        //public Dictionary<string, string> Settings { get; set; }

        private readonly IMemoryCache _cache;

        public UserSession(IConfiguration config, AppAuthService service, IMemoryCache memoryCache, AuthenticationStateProvider state, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            _cache = memoryCache;
            var authState = state.GetAuthenticationStateAsync().GetAwaiter().GetResult();
            if (!authState.User.Identity.IsAuthenticated)
            {
                Invalidate();
                return;
            }
            //Initialize usercontext
            // Get the UserInfo
            //User = service.GetUserByUsername(authState.User.Identity.Name).GetAwaiter().GetResult().First();
            // Get the IP Address
            IpAddress = GetIp();

            // Look for cache key.
            //if (_cache.TryGetValue(CacheKeys.GetUserSessionKey(User.UserId), out UserSession cacheEntry))
            //{
            //    UserFeatures = cacheEntry.UserFeatures;
            //    UserContext = cacheEntry.UserContext;
            //}
            //else
            //{
            //    Cache();
            //}
        }

        public void Invalidate()
        {
            //_cache.Remove(CacheKeys.GetUserSessionKey(User.UserId));
            //UserContext = new UserAppContext { DivisionId = _divisionid };
        }
        public void Cache()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(3600));
            //_cache.Set(CacheKeys.GetUserSessionKey(User.UserId), this, cacheEntryOptions);
        }

        private string GetIp()
        {
            try
            {
                //first try to get IP address from the forwarded header
                if (_accessor.HttpContext.Request.Headers != null)
                {
                    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                    //connecting to a web server through an HTTP proxy or load balancer
                    var forwardedHeader = _accessor.HttpContext.Request.Headers["X-Forwarded-For"];
                    if (!StringValues.IsNullOrEmpty(forwardedHeader)) return forwardedHeader.FirstOrDefault();
                }

                //if this header not exists try get connection remote IP address
                if (_accessor.HttpContext.Connection.RemoteIpAddress != null)
                {
                    return _accessor.HttpContext.Connection.RemoteIpAddress.ToString() == "::1" ? "LOCALPC" : _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                }
            }
            catch//Suppress error getting the ip
            {
                return "ERROR";
            }
            return "UNKNOWN";
        }
    }

}
