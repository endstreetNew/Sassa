using Microsoft.Extensions.Configuration;
using Sassa.eForms.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Sassa.eForms.Services
{

    public class SMSSender
    {
        private readonly API Api;

        public SMSSender(IHttpClientFactory clientFactory)
        {
            Api = new API(clientFactory.CreateClient("SMSClient"));
        }
        public async Task<SMSResponse> SendSMSAsync(string toCell, string Message)
        {
            if (!toCell.StartsWith("0"))
            {
                toCell = toCell.Replace("+27","0");
            }
            SMSRequest request = new SMSRequest { reference = new Random().Next(1000, 9999).ToString().Trim(), cellNumber = toCell, message = Message };
            return await Api.PostSMS("api/callcentre/sms", request).ConfigureAwait(false);
        }
    }

    public class VodacomSender
    {
        private readonly API Api;
        private IConfiguration config;

        public VodacomSender(IHttpClientFactory clientFactory, IConfiguration _config)
        {
            Api = new API(clientFactory.CreateClient("SMSClient"));
            config = _config;
        }
        public async Task<String> SendSMSAsync(string toCell, string message)
        {
            if (toCell.StartsWith("0"))
            {
                toCell = "+27" + toCell.Substring(1);
            }
            ///send?number=+27721234567&message=Message+Text
            var apicall = $"send?username={config["SMSService:UserName"]}&password={config["SMSService:Password"]}&number={HttpUtility.UrlEncode(toCell)}&message={HttpUtility.UrlEncode(message)}";
            return await Api.GetVodacomResult<String>(apicall).ConfigureAwait(false);
        }
        public bool ValidateCode(string code, string response)
        {

            return true;
        }
    }
}
