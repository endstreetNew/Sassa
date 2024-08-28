using Microsoft.AspNetCore.Mvc;

namespace Sassa.BRM.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class BrmFilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public BrmFilesController(IWebHostEnvironment env)
        {
            _env = env;
        }
        ///// <summary>
        ///// Get Phisical File .
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("{filename}")]
        //public async Task<IActionResult> Get(string filename)
        //{
        //    //string url = "{0}{1}/{2}";
        //    string url = $"{this.Request.Scheme}://{this.Request.Host}/brmFiles/{filename}";

        //    return Redirect(url);
        //}

        // GET api/file/downlaod
        /// <summary>
        /// Return a locally stored file based on id to the requesting client
        /// </summary>
        /// <param name="id">unique identifier for the requested file</param>
        /// <returns>an IAction Result</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Download(string id)
        {
            string path = Path.Combine(_env.ContentRootPath + "//brmFiles", id);

            if (System.IO.File.Exists(path))
            {
                // Get all bytes of the file and return the file with the specified file contents 
                byte[] b = await System.IO.File.ReadAllBytesAsync(path);
                return File(b, "application/octet-stream");
            }
            else
            {
                // return error if file not found
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
    }
}
