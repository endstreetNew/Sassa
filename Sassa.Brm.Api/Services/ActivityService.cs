using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;
using System.Diagnostics;

namespace Sassa.BRM.Api.Services;

public class ActivityService(IDbContextFactory<ModelContext> dbContextFactory)
{

    #region Activity
    public void SaveActivity(string action, string srdNo, decimal? lcType, string Activity, string regionId, decimal officeId, string samName, string UniqueFileNo = "")
    {
        try
        {
            using (var _context = dbContextFactory.CreateDbContext())
            {
                string area = action + GetFileArea(srdNo, lcType);
                DcActivity activity = new DcActivity { ActivityDate = DateTime.Now, RegionId = regionId, OfficeId = officeId, Userid = 0, Username = samName, Area = area, Activity = Activity, Result = "OK", UnqFileNo = UniqueFileNo };
                _context.DcActivities.Add(activity);
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    public string GetFileArea(string srdNo, decimal? lcType)
    {
        if (!string.IsNullOrEmpty(srdNo)) return "-SRD";
        if (lcType != null) return "-LC";
        return "-File";
    }
    #endregion

}
