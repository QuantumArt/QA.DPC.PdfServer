using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.WebApi.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    
    public class PdfController : BaseController
    {
        private readonly IHtmlGenerator _htmlGenerator;


        private readonly ILogger<PdfController> _logger;
        //private readonly PdfStaticFilesSettings _pdfStaticFilesSettings;

        public PdfController(IHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings, IOptions<PdfSettings> pdfSettings, ILogger<PdfController> logger)
        {
            _htmlGenerator = htmlGenerator;
            _logger = logger;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
            _pdfPageSettings = pdfSettings.Value;

        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        [HttpGet("{mode}/{id}")]
        public async Task<ActionResult> Get(int id, string category, int? templateId, bool asHtml, bool attachment, int? regionId, bool forceDownload, string mode = "live" )
        {
            try
            {
                var siteMode = ParseSiteMode(mode);

                var generatedHtml = await _htmlGenerator.GenerateHtml(id, category, templateId, regionId, siteMode, forceDownload);
                return GetGenerationActionResult(attachment, asHtml, generatedHtml);
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
    }
}
