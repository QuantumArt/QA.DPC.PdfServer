using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.DPC.PDFServer.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private readonly IPdfTemplateSelector _pdfTemplateSelector;

        public PdfController(IPdfTemplateSelector pdfTemplateSelector)
        {
            _pdfTemplateSelector = pdfTemplateSelector;
        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        public async Task<string> Get(int id, string category)
        {
            var pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateId(id, category);
            return "value";
        }
        
    }
}
