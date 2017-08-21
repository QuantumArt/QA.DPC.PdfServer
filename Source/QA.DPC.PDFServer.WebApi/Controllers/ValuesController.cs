using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IDpcApiClient _dpcApiClient;
        private readonly IPdfTemplateSelector _pdfTemplateSelector;

        public ValuesController(IDpcApiClient dpcApiClient, IPdfTemplateSelector pdfTemplateSelector)
        {
            _dpcApiClient = dpcApiClient;
            _pdfTemplateSelector = pdfTemplateSelector;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            //var pdfTemplates = await _dpcApiClient.GetProducts<DpcPdfTemplate>("pdf", new[] {2213768, 2213769}, new[] {"*"});
            var pdfTemplate = await  _pdfTemplateSelector.GetPdfTemplateId(2203747, "print");
            var productJson = await _dpcApiClient.GetProductJson(2203747);
            return new string[] { productJson};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
