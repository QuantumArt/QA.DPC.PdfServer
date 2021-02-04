using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.WebApi.Controllers;

namespace QA.DPC.PdfServer.RoamingApi
{
    public class RoamingController : BaseController
    {
        private readonly RoamingHtmlGenerator _htmlGenerator;

        public RoamingController(RoamingHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings, IOptions<PdfSettings> pdfSettings, IServiceProvider serviceProvider)
        {
            _htmlGenerator = htmlGenerator;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
            _pdfPageSettings = pdfSettings.Value;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [HttpGet("{countryCode}")]
        public async Task<ActionResult> Get(string customerCode, string countryCode, int? id, string category, int? templateId, bool asHtml, bool attachment, bool forceDownload, bool isB2B, string mode = "live")
        {
            try
            {

                var siteMode = ParseSiteMode(mode);
                var generatedHtml = await _htmlGenerator.GenerateRoamingHtml(customerCode, category,id, countryCode, isB2B, templateId, siteMode, forceDownload);
                return GetGenerationActionResult(attachment, asHtml, generatedHtml);
            }
            catch (GetProductJsonException ex)
            {
                Logger.Error(ex, "Error while getting product json");
                return new JsonResult(new { success = false, error = $"Error while getting product json: {ex.Message}" });
            }
            catch (TemplateNotFoundException ex)
            {
                Logger.Error(ex, "Template not found");
                return new JsonResult(new { success = false, error = $"Template not found: {ex.Message}" });
            }
            catch (HtmlGenerationException ex)
            {
                Logger.Error(ex, "Error while generating html");
                return new JsonResult(new { success = false, error = $"Error while generating html: {ex.Message}" });
            }
            catch (PdfGenerationException ex)
            {
                Logger.Error(ex, "Error while generating pdf");
                return new JsonResult(new { success = false, error = $"Error while generating pdf: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "General error");
                return new JsonResult(new { success = false, error = ex.Message });
            }

        }
    }
}
