using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using QA.DPC.PDFServer.Services.Interfaces;

namespace QA.DPC.PdfServer.DevApi
{
    [Route("api/[controller]")]
    [Route("api/{customerCode}/[controller]")]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationServiceClient _configurationServiceClient;
        private readonly IDpcDbClient _dpcDbClient;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ConfigurationController(IConfigurationServiceClient configurationServiceClient, IDpcDbClient dpcDbClient)
        {
            _configurationServiceClient = configurationServiceClient;
            _dpcDbClient = dpcDbClient;
        }

        [HttpGet]
        public async Task<ActionResult> Get(string customerCode)
        {
            try
            {
                var configuration = await _configurationServiceClient.GetCustomerCodeConfiguration(customerCode);
                return new JsonResult(new {success = true, configuration});
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while getting customer code configuration");
                return new JsonResult(new {success = false, error = $"Error while getting customer code configuration: {ex.Message}" });
            }
        }

        [HttpGet("highloadapitoken")]
        public async Task<ActionResult> GetHighloadApiToken(string customerCode)
        {
            try
            {
                var token = await _dpcDbClient.GetCachedHighloadApiAuthToken(customerCode);
                return new JsonResult(new {success = true, token});
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while getting customer code configuration");
                return new JsonResult(new {success = false, error = $"Error while getting auth token: {ex.Message}" });
            }
        }
    }
}