using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.WebApi.Logging;

namespace QA.DPC.PDFServer.WebApi.Controllers
{
    #warning Удалить/закомментить этот контроллер! нужен только в процессе разработки/тестирования, в прод его выставлять небезопасно.
    public class ConfigurationController : BaseController
    {
        private readonly IConfigurationServiceClient _configurationServiceClient;
        private readonly IDpcDbClient _dpcDbClient;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IConfigurationServiceClient configurationServiceClient, IDpcDbClient dpcDbClient,  ILogger<ConfigurationController> logger)
        {
            _configurationServiceClient = configurationServiceClient;
            _dpcDbClient = dpcDbClient;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<ActionResult> Get(string customerCode)
        {
            try
            {
                var configuration = await _configurationServiceClient.GetCustomerCodeConfiguration(customerCode);
//                var configurationJson = await _configurationServiceClient.GetCustomerCodeConfigurationJson(customerCode);
                return new JsonResult(new {success = true, configuration = configuration});
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GetCustomerCodeConfiguration, ex, "Error while getting customer code configuration");
                return new JsonResult(new {success = false, error = $"Error while getting customer code configuration: {ex.Message}" });
            }
        }

        [HttpGet("highloadapitoken")]
        public async Task<ActionResult> GetHighloadApiToken(string customerCode)
        {
            try
            {
                var token = await _dpcDbClient.GetCachedHighloadApiAuthToken(customerCode);
//                var configurationJson = await _configurationServiceClient.GetCustomerCodeConfigurationJson(customerCode);
                return new JsonResult(new {success = true, token});
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GetCustomerCodeConfiguration, ex, "Error while getting customer code configuration");
                return new JsonResult(new {success = false, error = $"Error while getting auth token: {ex.Message}" });
            }
        }
    }
}