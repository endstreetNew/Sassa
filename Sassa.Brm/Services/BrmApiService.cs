using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using System.Diagnostics;
using System.Text.Json;


namespace Sassa.BRM.Services;

public class BrmApiService(IHttpClientFactory _httpClientFactory, IConfiguration config)
{
    string _brmApiUrl = config["Urls:BrmApi"]!;
    #region Application
    public async Task<DcFile?> PostApplication(Application application)
    {
        var client = _httpClientFactory.CreateClient("BrmApplication");
        var serializationOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true
        };
        var result = await client.PostAsJsonAsync(_brmApiUrl + "ApplicationV2", application, serializationOptions);

        var apiResponse = await result.Content.ReadFromJsonAsync<ApiResponse<DcFile>>();
        if (apiResponse is null) throw new Exception("Fatal APi Error.");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception(apiResponse.ErrorMessage);
        }
        return apiResponse.Data;
    }

    #region DcFile
    public async Task<DcFile?> PostDcFile(DcFile dcfile)
    {
        var client = _httpClientFactory.CreateClient("BrmApplication");
        var serializationOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true
        };
        var result = await client.PostAsJsonAsync(_brmApiUrl + "DcFile", dcfile, serializationOptions);
        try
        {
            return await result.Content.ReadFromJsonAsync<DcFile>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Api error.{ex.Message}");
        }
    }
    #endregion
    #endregion
    #region Activity

    public void PostActivity(DcActivity activity)
    {
        var client = _httpClientFactory.CreateClient("BrmActivity");
        _ = client.PostAsJsonAsync(_brmApiUrl + "Activity", activity);
    }

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
