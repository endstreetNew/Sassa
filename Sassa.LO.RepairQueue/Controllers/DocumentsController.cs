using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sassa.LO.RepairQueue.Services;

namespace Sassa.LO.RepairQueue.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        DocumentService _docService;
        public DocumentsController(DocumentService docService)
        {
            _docService = docService;
        }   
        [HttpGet("pdf/{reference}")]
        public IActionResult GetPdf(string reference)
        {
            string fileName = _docService.GetFirstDocument(reference);
            var bytes = System.IO.File.ReadAllBytes(fileName);
            return File(bytes, "application/pdf");
        }

    }
}
