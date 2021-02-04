using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    public class ProductJsonMapperController : BaseController
    {
        private readonly IProductJsonMapper _productJsonMapper;

        public ProductJsonMapperController(IProductJsonMapper productJsonMapper)
        {
            _productJsonMapper = productJsonMapper;
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
                Logger.Error(ex, "Error while getting product json");
                return new JsonResult(new { success = false, error = $"Error while getting product json: {ex.Message}" });
            }
            catch (TemplateNotFoundException ex)
            {
                Logger.Error(ex, "Template not found");
                return new JsonResult(new { success = false, error = $"Template not found: {ex.Message}" });
            }
            catch (ProductMappingException ex)
            {
                Logger.Error(ex, "Error while mapping product");
                return new JsonResult(new { success = false, error = $"Error while generating mapping product: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "General error");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }



    }
}
