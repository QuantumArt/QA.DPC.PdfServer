using System;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NLog;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Route("api/{customerCode}/[controller]")]
    public class BaseController : Controller
    {
        protected PdfStaticFilesSettings _pdfStaticFilesSettings;
        protected PdfSettings _pdfPageSettings;
        protected IServiceProvider _serviceProvider;
        protected readonly ILogger Logger;

        public BaseController()
        {
            Logger = LogManager.GetLogger(GetType().ToString());
        }

        protected static SiteMode ParseSiteMode(string mode)
        {
            var siteMode = SiteMode.Unknown;

            if (mode.Equals("live", StringComparison.InvariantCultureIgnoreCase))
            {
                siteMode = SiteMode.Live;
            }
            if (mode.Equals("stage", StringComparison.InvariantCultureIgnoreCase))
            {
                siteMode = SiteMode.Stage;
            }
            if (siteMode == SiteMode.Unknown)
                throw new Exception("Unknown site mode; must be empty or \"stage\" or \"live\"");
            return siteMode;
        }

        protected ActionResult GetGenerationActionResult(bool attachment, bool asHtml, string generatedHtml)
        {
            if (asHtml)
            {
                if (attachment)
                {
                    var htmlBytes = Encoding.UTF8.GetBytes(generatedHtml);
                    Response.Headers.Add("Content-Type", "text/html");
                    return new FileContentResult(htmlBytes, "text/html");
                }
                return new JsonResult(new { success = true, generatedHtml = generatedHtml });
            }
            if (attachment)
            {
                var pdf = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml, _pdfPageSettings, _serviceProvider);
                Response.Headers.Add("Content-Type", "application/pdf");
                return new FileContentResult(pdf, "application/pdf");
            }
            var fileName = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml, _pdfPageSettings, _serviceProvider, _pdfStaticFilesSettings.RootOutputDirectory);
            return new JsonResult(new
            {
                success = true,
                pdfRelativePath = $"{_pdfStaticFilesSettings.DirectoryRelativePath}/{fileName}"
            });
        }
    }
}