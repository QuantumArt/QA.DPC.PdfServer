using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.WebApi.Logging;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    public class RoamingController : BaseController
    {
        private readonly IHtmlGenerator _htmlGenerator;
        private readonly ILogger<RoamingController> _logger;

        public RoamingController(IHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings, IOptions<PdfSettings> pdfSettings, ILogger<RoamingController> logger)
        {
            _htmlGenerator = htmlGenerator;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
            _pdfPageSettings = pdfSettings.Value;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("{countryCode}")]
        public async Task<ActionResult> Get(string countryCode, int? id, string category, int? templateId, bool asHtml, bool attachment, bool forceDownload, bool isB2B, string mode = "live")
        {
            try
            {

                var siteMode = ParseSiteMode(mode);
                var generatedHtml = await _htmlGenerator.GenerateRoamingHtml(category,id, countryCode, isB2B, templateId, siteMode, forceDownload);
                return GetGenerationActionResult(attachment, asHtml, generatedHtml);
            }
            catch (GetProductJsonException ex)
            {
                _logger.LogError(LoggingEvents.GetProduct, ex, "Error while getting product json");
                return new JsonResult(new { success = false, error = $"Error while getting product json: {ex.Message}" });
            }
            catch (TemplateNotFoundException ex)
            {
                _logger.LogError(LoggingEvents.TemplateNotFound, ex, "Template not found");
                return new JsonResult(new { success = false, error = $"Template not found: {ex.Message}" });
            }
            catch (HtmlGenerationException ex)
            {
                _logger.LogError(LoggingEvents.HtmlGeneration, ex, "Error while generating html");
                return new JsonResult(new { success = false, error = $"Error while generating html: {ex.Message}" });
            }
            catch (PdfGenerationException ex)
            {
                _logger.LogError(LoggingEvents.PdfGeneration, ex, "Error while generating pdf");
                return new JsonResult(new { success = false, error = $"Error while generating pdf: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.General, ex, "General error");
                return new JsonResult(new { success = false, error = ex.Message });
            }

        }
    }
}
