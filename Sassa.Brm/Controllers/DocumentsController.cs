using Microsoft.AspNetCore.Mvc;
using Sassa.Services;

namespace Sassa.BRM.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        DocumentService _docService;
        private readonly ILogger<DocumentsController> _logger;
        public DocumentsController(DocumentService docService, ILogger<DocumentsController> logger)
        {
            _docService = docService;
            _logger = logger;
        }
        [HttpGet("pdf/{reference}")]
        public IActionResult GetPdf(string reference)
        {
            try
            {
                string fileName = _docService.GetFirstDocument(reference);
                var bytes = System.IO.File.ReadAllBytes(fileName);
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serve PDF for reference {Reference}", reference);
                return Problem("Failed to read the requested file.");
            }
        }
        //[AllowAnonymous]
        //[HttpGet("reject/{reference}")]
        //public IActionResult RejectPdf(string reference)
        //{
        //    try
        //    {
        //        string fileName = _docService.GetFirstDocument(reference);
        //        //System.IO.File.Move(fileName, fileName.Replace("Pending", "Rejected"));
        //        System.IO.File.Move(fileName, Path.Combine(_rejectedDirectory, fileName));
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to serve PDF for reference {Reference}", reference);
        //        return Problem("Failed to read the requested file.");
        //    }
        //}
    }
}
