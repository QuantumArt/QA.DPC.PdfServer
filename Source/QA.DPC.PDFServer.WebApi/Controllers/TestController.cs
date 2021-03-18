using Microsoft.AspNetCore.Mvc;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    public class TestController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return Ok();
        }
    }
}