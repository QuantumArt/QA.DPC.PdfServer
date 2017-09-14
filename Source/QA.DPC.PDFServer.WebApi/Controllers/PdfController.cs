using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.WebApi.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private readonly IHtmlGenerator _htmlGenerator;
        private readonly ILogger<PdfController> _logger;
        private readonly PdfStaticFilesSettings _pdfStaticFilesSettings;

        public PdfController(IHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings, ILogger<PdfController> logger)
        {
            _htmlGenerator = htmlGenerator;
            _logger = logger;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        [HttpGet("{mode}/{id}")]
        public async Task<ActionResult> Get(int id, string category, int? templateId, bool asHtml, bool attachment, int? regionId, string mode = "live" )
        {
            try
            {
                var siteMode = ParseSiteMode(mode);

                var generatedHtml = await _htmlGenerator.GenerateHtml(id, category, templateId, regionId, siteMode);
                if (asHtml)
                {
                    if (attachment)
                    {
                        var htmlBytes = Encoding.UTF8.GetBytes(generatedHtml);
                        Response.Headers.Add("Content-Type", "text/html");
                        //Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.html; size={htmlBytes.Length.ToString()}");
                        return new FileContentResult(htmlBytes, "text/html");
                    }
                    return new JsonResult(new {success = true, generatedHtml = generatedHtml});
                }
                if (attachment)
                {
                    var pdf = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml);
                     Response.Headers.Add("Content-Type", "application/pdf");
                    //Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.pdf; size={pdf.Length.ToString()}");
                    return new  FileContentResult(pdf, "application/pdf");
                }
                var fileName = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml, _pdfStaticFilesSettings.RootOutputDirectory);
                return new JsonResult(new
                {
                    success = true,
                    pdfRelativePath = $"{_pdfStaticFilesSettings.DirectoryRelativePath}/{fileName}"
                });
            }
            catch (GetProductJsonException ex)
            {
                _logger.LogError(LoggingEvents.GetProduct, ex, "Error while getting product json");
                return new JsonResult(new {success = false, error = $"Error while getting product json: {ex.Message}"});
            }
            catch (TemplateNotFoundException ex)
            {
                _logger.LogError(LoggingEvents.TemplateNotFound, ex, "Template not found");
                return new JsonResult(new {success = false, error = $"Template not found: {ex.Message}"});
            }
            catch (HtmlGenerationException ex)
            {
                _logger.LogError(LoggingEvents.HtmlGeneration, ex, "Error while generating html");
                return new JsonResult(new {success = false, error = $"Error while generating html: {ex.Message}"});
            }
            catch (PdfGenerationException ex)
            {
                _logger.LogError(LoggingEvents.PdfGeneration, ex, "Error while generating pdf");
                return new JsonResult(new { success = false, error = $"Error while generating pdf: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.General, ex, "General error");
                return new JsonResult(new {success = false, error = ex.Message});
            }
        }

        private static SiteMode ParseSiteMode(string mode)
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
    }
}
