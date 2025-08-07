using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;

namespace Sassa.Brm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FasttrackController : ControllerBase
    {

        private readonly ApplicationService _brmService;
        IConfiguration _config;

        public FasttrackController(ApplicationService context, IConfiguration config)
        {
            _brmService = context;
            _config = config;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<DcFile>>> Fasttrack(FasttrackScan app)
        {
            DcFile result = new DcFile();

            result.UpdatedByAd = _config.GetValue<string>("BrmUser")!;

            

            ApiResponse<DcFile> response = new ApiResponse<DcFile>();
            try
            {
                //todo: 1 using the reference populate the brm record
                //Validate the application
                //var ValidApplcation = _brmService.ValidateApplcation(app);
                //if (ValidApplcation != "") throw new Exception(ValidApplcation);

                //if (app.BrmUserName == "SVC_BRM_LO")
                //{
                //    response.Data = await _brmService.ValidateApiAndInsert(app, "Inserted via API.");
                //}
                //else
                //{
                //    response.Data = await _brmService.CreateBRM(app, "Inserted via BRM Capture.");
                //}
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle both ValidationException and InternalServerErrorException here
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            return BadRequest(response);

        }
    }
}
