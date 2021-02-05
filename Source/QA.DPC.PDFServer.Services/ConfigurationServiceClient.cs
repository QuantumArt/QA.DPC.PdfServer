using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.DPC.PDFServer.Services
{
    public class ConfigurationServiceClient : IConfigurationServiceClient
    {
        private readonly ConfigurationServiceSettings _settings;

        public ConfigurationServiceClient(IOptions<ConfigurationServiceSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<CustomerConfiguration> GetCustomerCodeConfiguration(string customerCode)
        {
            try
            {
                DBConnector.ConfigServiceUrl = _settings.BaseUrl;
                DBConnector.ConfigServiceToken = _settings.XAuthToken;
                var configuration = await DBConnector.GetQpConfiguration();
                var customer = configuration.Customers.SingleOrDefault(n => n.Name == customerCode);
                if (customer == null)
                {
                    throw new GetCustomerCodeConfigurationException(
                        $"Customer code \"{customerCode}\" not found");                  
                }
                return customer;
            }
            catch (GetCustomerCodeConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GetCustomerCodeConfigurationException(
                    $"Get configuration for customer code = \"{customerCode}\" error.", ex);
            }
        }
    }
}