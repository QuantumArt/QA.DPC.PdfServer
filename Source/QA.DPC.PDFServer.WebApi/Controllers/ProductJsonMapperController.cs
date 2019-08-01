using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.WebApi.Logging;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    public class ProductJsonMapperController : BaseController
    {
        private readonly IProductJsonMapper _productJsonMapper;
        private readonly ILogger<ProductJsonMapperController> _logger;

        public ProductJsonMapperController(IProductJsonMapper productJsonMapper, ILogger<ProductJsonMapperController> logger)
        {
            _productJsonMapper = productJsonMapper;
            _logger = logger;
        }


        [HttpGet("{id}")]
        [HttpGet("{mode}/{id}")]
        public async Task<ActionResult> Get(string customerCode, int id, int? mapperId, int? templateId, string category, bool forceDownload, string mode = "live")
        {
            try
            {
                var siteMode = ParseSiteMode(mode);
                var mappedProduct = await _productJsonMapper.MapProductJson(customerCode, id, category, mapperId, templateId, forceDownload, siteMode);
                return new JsonResult(new {success = true, jsonString = mappedProduct});
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
            catch (ProductMappingException ex)
            {
                _logger.LogError(LoggingEvents.HtmlGeneration, ex, "Error while mapping product");
                return new JsonResult(new { success = false, error = $"Error while generating mapping product: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.General, ex, "General error");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }



    }
}
