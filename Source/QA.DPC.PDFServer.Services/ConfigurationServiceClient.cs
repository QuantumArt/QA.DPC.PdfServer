using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class ConfigurationServiceClient : IConfigurationServiceClient
    {
        private ConfigurationServiceSettings _settings;

        public ConfigurationServiceClient(IOptions<ConfigurationServiceSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<CustomerCodeConfiguration> GetCustomerCodeConfiguration(string customerCode)
        {
            try
            {
                var json = await GetCustomerCodeConfigurationJson(customerCode);
                return JsonConvert.DeserializeObject<CustomerCodeConfiguration>(json);
            }
            catch (GetCustomerCodeConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GetCustomerCodeConfigurationException($"Get configuration for customer code = \"{customerCode}\" error.", ex);
            }
            
        }

        public async Task<string> GetCustomerCodeConfigurationJson(string customerCode)
        {
            var url = GetCustomerCodeConfigurationUrl(customerCode);
            try
            {
                return await MakeRequest(url);
            }
            catch (Exception ex)
            {
                throw new GetCustomerCodeConfigurationException($"Get configuration for customer code = \"{customerCode}\" error.", ex);
            }
        }

        private async Task<string> MakeRequest(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.XAuthToken);

                return await client.GetStringAsync(url);
            }
        }

        private string GetCustomerCodeConfigurationUrl(string customerCode)
        {
            return $"{_settings.BaseUrl}/api/v1/customers/{customerCode}";
        }
    }
}