using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sassa.BRM.Models;
using Sassa.Brm.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Sassa.Activity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController(IDbContextFactory<ModelContext> _contextFactory) : ControllerBase
{

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> PostActivity(DcActivity activity)
    {

        ApiResponse<string> response = new ApiResponse<string>();
        try
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                try
                {
                    _context.DcActivities.Add(activity);
                    await _context.SaveChangesAsync();
                    response.Success = true;
                }
                catch
                {
                    //just ignoring activity post errors for now.
                }
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }
        return Ok(response);
    }
}
