
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sassa.eForms.Services
{

    public class AppAuthService
    {
        private API Api;
        public AppAuthService(IHttpClientFactory clientFactory)
        {
            Api = new API(clientFactory.CreateClient("AppAuthService"));
        }
        //GetUserEffectiveFeatures/{applicationCode}/{userId}
        //public async Task<IEnumerable<Feature>> GetUserFeatures(string userId)
        //{
        //    //Todo: remove this workaround for AppAuth issue
        //    string apicall = "getfeatures";
        //    return await Api.GetResult<IEnumerable<Feature>>(apicall).ConfigureAwait(false);
        //    //string apicall = $"GetUserEffectiveFeatures/{AppData.AppCode}/{userId}";
        //    //return await Api.GetResult<IEnumerable<Feature>>(apicall).ConfigureAwait(false);
        //}

        //public async Task<IEnumerable<Feature>> GetFeatures()
        //{
        //    string apicall = "getfeatures";
        //    return await Api.GetResult<IEnumerable<Feature>>(apicall).ConfigureAwait(false);
        //}

        /////GetUserByUsername/{applicationCode}/{username}
        //public async Task<IEnumerable<User>> GetUserByUsername(string username)
        //{
        //    string apicall = "GetUserByUsername";
        //    apicall = $"{apicall}/{AppData.AppCode}/{username}";
        //    return await Api.GetResult<IEnumerable<User>>(apicall).ConfigureAwait(false);
        //}

        //public async Task<IEnumerable<UserPermission>> GetUserPermissions(string userid)
        //{
        //    string apicall = "GetUserEffectivePermissions";
        //    apicall = $"{apicall}/{AppData.AppCode}/{userid}";
        //    return await Api.GetResult<IEnumerable<UserPermission>>(apicall).ConfigureAwait(false);
        //}

        //public async Task<IEnumerable<User>> GetApplicationUsers()
        //{
        //    string apicall = "GetUsersByApplication";
        //    apicall = $"{apicall}/" + AppData.AppCode;
        //    return await Api.GetResult<IEnumerable<User>>(apicall).ConfigureAwait(false);
        //}
    }
}
