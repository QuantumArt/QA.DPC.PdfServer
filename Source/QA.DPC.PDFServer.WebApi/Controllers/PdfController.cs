﻿using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Settings;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private readonly IHtmlGenerator _htmlGenerator;
        private readonly PdfStaticFilesSettings _pdfStaticFilesSettings;

        public PdfController(IHtmlGenerator htmlGenerator, IOptions<PdfStaticFilesSettings> pdfStaticFilesSettings)
        {
            _htmlGenerator = htmlGenerator;
            _pdfStaticFilesSettings = pdfStaticFilesSettings.Value;
        }

        // GET api/pdf/5?category=print
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, string category, bool asHtml, bool attachment )
        {
            try
            {
                var generatedHtml = await _htmlGenerator.GenerateHtml(id, category);
                if (asHtml)
                {
                    if (attachment)
                    {
                        var htmlBytes = Encoding.UTF8.GetBytes(generatedHtml);
                        Response.Headers.Add("Content-Type", "text/html");
                        Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.html; size={htmlBytes.Length.ToString()}");
                        return new FileContentResult(htmlBytes, "text/html");
                    }
                    return new JsonResult(new {success = true, generatedHtml = generatedHtml});
                }
                if (attachment)
                {
                    var pdf = PdfGenerator.PdfGenerator.GeneratePdf(generatedHtml);
                     Response.Headers.Add("Content-Type", "application/pdf");
                    Response.Headers.Add("Content-Disposition", $"attachment;filename={id}_{category}.pdf; size={pdf.Length.ToString()}");
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
                return new JsonResult(new {success = false, error = $"Error while getting product json: {ex.Message}"});
            }
            catch (TemplateNotFoundException ex)
            {
                return new JsonResult(new {success = false, error = $"Template not found: {ex.Message}"});
            }
            catch (HtmlGenerationException ex)
            {
                return new JsonResult(new {success = false, error = $"Error while generating html: {ex.Message}"});
            }
            catch (PdfGenerationException ex)
            {
                return new JsonResult(new { success = false, error = $"Error while generating pdf: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new {success = false, error = ex.Message});
            }
        }
    }
}
