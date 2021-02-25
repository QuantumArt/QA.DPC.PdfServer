using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    
    public class ProductController : BaseController
    {
        private readonly IHtmlGenerator _htmlGenerator;

        public ProductController(IHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings, IOptions<PdfSettings> pdfSettings, IServiceProvider serviceProvider)
        {
            _htmlGenerator = htmlGenerator;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
            _pdfPageSettings = pdfSettings.Value;
            _serviceProvider = serviceProvider;
        }

        // GET api/product/5?category=print
        [HttpGet("{id}")]
        [HttpGet("{mode}/{id}")]
        public async Task<ActionResult> Get(string customerCode, int id, string category, int? templateId, bool asHtml, bool attachment, int? regionId, bool forceDownload, string mode = "live" )
        {
            try
            {
                var siteMode = ParseSiteMode(mode);

                var generatedHtml = await _htmlGenerator.GenerateHtml(customerCode, id, category, templateId, regionId, siteMode, forceDownload);
                return GetGenerationActionResult(attachment, asHtml, generatedHtml);
            }
            catch (GetProductJsonException ex)
            {
                Logger.Error(ex, "Error while getting product json");
                return new JsonResult(new {success = false, error = $"Error while getting product json: {ex.InnerException?.Message ?? ex.Message}"});
            }
            catch (TemplateNotFoundException ex)
            {
                Logger.Error(ex, "Template not found");
                return new JsonResult(new {success = false, error = $"Template not found: {ex.Message}"});
            }
            catch (HtmlGenerationException ex)
            {
                Logger.Error(ex, "Error while generating html");
                return new JsonResult(new {success = false, error = $"Error while generating html: {ex.Message}"});
            }
            catch (PdfGenerationException ex)
            {
                Logger.Error(ex, "Error while generating pdf");
                return new JsonResult(new { success = false, error = $"Error while generating pdf: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "General error");
                return new JsonResult(new {success = false, error = ex.Message});
            }
        }
    }
}
