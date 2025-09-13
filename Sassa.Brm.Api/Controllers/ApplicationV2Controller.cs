using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
//using System.ServiceModel.Channels;

namespace Sassa.BRM.Controller
{
    [Route("[controller]")]
    [ApiController]

    public class ApplicationV2Controller : ControllerBase
    {

        private readonly ApplicationService _brmService;
        IConfiguration _config;

        public ApplicationV2Controller(ApplicationService context, IConfiguration config)
        {
            _brmService = context;
            _config = config;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<DcFile>>> PostApplication(ApplicationModel app)
        {
            DcFile result = new DcFile();

            if (string.IsNullOrEmpty(app.BrmUserName))
            {
                app.BrmUserName = _config.GetValue<string>("BrmUser")!;
            }

            ApiResponse<DcFile> response = new ApiResponse<DcFile>();
            try
            {
                var ValidApplcation = _brmService.ValidateApplcation(app);
                if (ValidApplcation != "") throw new Exception(ValidApplcation);

                if (app.BrmUserName == "SVC_BRM_LO")
                {
                    response.Data = await _brmService.ValidateApiAndInsert(app, "Inserted via API.");
                }
                else
                {
                    response.Data = await _brmService.CreateBRM(app, "Inserted via BRM Capture.");
                }
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
        [HttpGet("healthcheck")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<bool>> Healthcheck()
        {
            return Ok(true);
        }

    }
}
