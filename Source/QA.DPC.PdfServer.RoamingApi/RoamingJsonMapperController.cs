using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.DPC.PdfServer.RoamingApi.Interfaces;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.WebApi.Controllers;

namespace QA.DPC.PdfServer.RoamingApi
{
    public class RoamingJsonMapperController : BaseController
    {
        private readonly IRoamingJsonMapper _roamingJsonMapper;

        public RoamingJsonMapperController(IRoamingJsonMapper roamingJsonMapper)
        {
            _roamingJsonMapper = roamingJsonMapper;
        }

        [HttpGet("{id}")]
        [HttpGet("{mode}/{id}")]
        public async Task<ActionResult> Get(string customerCode, int? id, string countryCode, int? mapperId, int? templateId, string category, bool forceDownload, bool isB2b, string mode = "live")
        {
            try
            {
                var siteMode = ParseSiteMode(mode);
                var mappedProduct = await _roamingJsonMapper.MapRoamingCountryJson(customerCode, id, countryCode, category, isB2b, mapperId, templateId, forceDownload, siteMode);
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
