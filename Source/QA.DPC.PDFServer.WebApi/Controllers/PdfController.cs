using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private readonly IHtmlGenerator _htmlGenerator;
        private readonly IHostingEnvironment _env;

        public PdfController(IHtmlGenerator htmlGenerator, IHostingEnvironment env)
        {
            _htmlGenerator = htmlGenerator;
            _env = env;
        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, string category, bool asHtml)
        {
            try
            {
                var generatedHtml = await _htmlGenerator.GenerateHtml(id, category);
                if (asHtml)
                {
                    return new JsonResult(new {success = true, generatedHtml = generatedHtml});
                }
                var outputDir = Path.Combine(_env.WebRootPath, "Output");
                var fileName = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml, outputDir);
                return new JsonResult(new {success = true, pdfRelativePath = $"/Output/{fileName}"});
            }
            catch (Exception ex)
            {
                return new JsonResult(new {success = false, error = ex.Message});
            }
        }
    }
}
