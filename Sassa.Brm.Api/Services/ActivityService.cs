using System.Diagnostics;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Sassa.Brm.Common.Models;
using Sassa.BRM.Models;

namespace Sassa.BRM.Api.Services;

public class ActivityService(IHttpClientFactory _httpClientFactory, IConfiguration config)
{
    string _brmApiUrl = config["Urls:BrmApi"]!;
    public void PostActivity(DcActivity activity)
    {
        var client = _httpClientFactory.CreateClient("Brm");
        _ = client.PostAsJsonAsync(_brmApiUrl + "Activity",activity);
    }
    #region Activity

    public void CreateActivity(string action, string srdNo, decimal? lcType, string Activity, string regionId, decimal officeId, string samName, string UniqueFileNo = "")
    {
        try
        {
            string area = action + GetFileArea(srdNo, lcType);
            DcActivity activity = new DcActivity { ActivityDate = DateTime.Now, RegionId = regionId, OfficeId = officeId, Userid = 0, Username = samName, Area = area, Activity = Activity, Result = "OK", UnqFileNo = UniqueFileNo };
            PostActivity(activity);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public string GetFileArea(string srdNo, decimal? lcType)
    {
        if (!string.IsNullOrEmpty(srdNo))
        {
            return "-SRD";
        }
        if (lcType != null)
        {
            return "-LC";
        }
        return "-File";
    }
    #endregion

}
