using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.BRM.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FasttrackController : ControllerBase
    {

        private readonly FasttrackService _ftService;
        private readonly LoService _loService;
        IConfiguration _config;

        public FasttrackController(FasttrackService context, IConfiguration config,LoService loService)
        {
            _ftService = context;
            _config = config;
            _loService = loService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<DcFile>>> Fasttrack(FasttrackScan app)
        {
            DcFile result = new DcFile();

            result.UpdatedByAd = _config.GetValue<string>("BrmUser")!;
            string ScanFolder = _config.GetValue<string>("Urls:ScanFolderRoot")!;

            ApiResponse<DcFile> response = new ApiResponse<DcFile>();
            try
            {
                _ftService.SetScanFolder(ScanFolder);
                await _ftService.ProcessLoRecord(app);
                await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = app.LoReferece, ValidationDate = DateTime.Now,Validationresult = "Ok"});
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = app.LoReferece, ValidationDate = DateTime.Now, Validationresult = ex.Message });
                // Handle both ValidationException and InternalServerErrorException here
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            return BadRequest(response);

        }
    }
}
