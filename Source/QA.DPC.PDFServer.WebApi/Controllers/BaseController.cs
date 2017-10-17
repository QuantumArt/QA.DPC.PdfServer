using System;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class BaseController : Controller
    {
        protected PdfStaticFilesSettings _pdfStaticFilesSettings;

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
                    //Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.html; size={htmlBytes.Length.ToString()}");
                    return new FileContentResult(htmlBytes, "text/html");
                }
                return new JsonResult(new { success = true, generatedHtml = generatedHtml });
            }
            if (attachment)
            {
                var pdf = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml);
                Response.Headers.Add("Content-Type", "application/pdf");
                //Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.pdf; size={pdf.Length.ToString()}");
                return new FileContentResult(pdf, "application/pdf");
            }
            var fileName = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml, _pdfStaticFilesSettings.RootOutputDirectory);
            return new JsonResult(new
            {
                success = true,
                pdfRelativePath = $"{_pdfStaticFilesSettings.DirectoryRelativePath}/{fileName}"
            });
        }
    }
}