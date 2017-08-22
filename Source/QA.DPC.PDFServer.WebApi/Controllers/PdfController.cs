using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IDpcApiClient _client;
        private readonly IPdfTemplateSelector _pdfTemplateSelector;

        public PdfController(IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator;

            
        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, string category)
        {
            var generatedHtml = await _htmlGenerator.GenerateHtml(id, category);
            var pdf = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml);
            //Response.Headers.Add("Content-Type", "application/pdf");
            //Response.Headers.Add("Content-Disposition",
            //    $"attachment;filename={id}_{category}.pdf; size={pdf.Length.ToString()}");
             
            //Response
               return new  FileContentResult(pdf, "application/pdf");
            //return "value";
        }

       
    }
}
