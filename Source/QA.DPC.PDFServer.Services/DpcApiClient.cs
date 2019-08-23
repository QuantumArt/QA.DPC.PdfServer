using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public class DpcApiClient : IDpcApiClient
    {
        private readonly IDpcDbClient _dpcDbClient;
        private readonly DpcApiSettings _settings;
        private readonly IHttpClientFactory _factory;

        public DpcApiClient(IOptions<DpcApiSettings> settings, IDpcDbClient dpcDbClient, IHttpClientFactory factory)
        {
            _dpcDbClient = dpcDbClient;
            _settings = settings.Value;
            _factory = factory;
        }


        public async Task<string> GetProductJson(string customerCode, string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var url = GetProductJsonDownloadUrl(customerCode, slug, parameters, allFields, siteMode, fields);
            try
            {
                return await MakeRequest(customerCode, url);
            }
            catch (Exception ex)
            {
                throw new GetProductJsonException($"Get product json with slug = {slug}", ex);
            }
        }

        

        public async Task<string> GetProductJson(string customerCode, int id, SiteMode siteMode)
        {
            return await GetProductJson(customerCode, id, false, siteMode);
        }

        public async Task<string> GetProductJson(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var url = GetProductJsonDownloadUrl(customerCode, id, allFields, siteMode, fields);
            try
            {
                return await MakeRequest(customerCode, url);
            }
            catch (Exception ex)
            {
                throw new GetProductJsonException($"Get product json with id = {id} error.", ex);
            }
        }

        public async Task<T> GetProduct<T>(string customerCode, int id, SiteMode siteMode)
        {
            return await GetProduct<T>(customerCode, id, false, siteMode);
        }

        public async Task<T> GetProduct<T>(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var productJson = await GetProductJson(customerCode, id, allFields, siteMode, fields);
            return JsonConvert.DeserializeObject<T>(productJson);
        }

        public async Task<T> GetProduct<T>(string customerCode, string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var productJson = await GetProductJson(customerCode, slug, parameters, allFields, siteMode, fields);
            return JsonConvert.DeserializeObject<T>(productJson);
        }

        public async Task<RegionTags[]> GetRegionTags(string customerCode, int productId, SiteMode siteMode)
        {
            var url = GetRegionTagsUrl(customerCode, productId, siteMode);
            var json = await MakeRequest(customerCode, url);
            return JsonConvert.DeserializeObject<RegionTags[]>(json);
        }

        

        public async Task<string> GetProductsJson(string customerCode, string productType, int[] ids, SiteMode siteMode, string[] fields = null)
        {
            
            var url = GetProductsJsonDownloadUrl(customerCode, productType, ids, siteMode);

            if (fields != null && fields.Any())
            {
                url += $"&fields={string.Join(",", fields)}";
            }

            return await MakeRequest(customerCode, url);
        }

       

        public async Task<IEnumerable<T>> GetProducts<T>(string customerCode, string productType, int[] ids, SiteMode siteMode, string[] fields = null)
        {
            var productsJsons = await GetProductsJson(customerCode, productType, ids, siteMode, fields);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(productsJsons);
        }
        
        private string GetProductsJsonDownloadUrl(string customerCode, string productType, int[] ids, SiteMode siteMode)
        {
            var customerCodePart = GetCustomerCodeUrlPart(customerCode);
            //вариант для получения одним запросом(когда починят).
            return $"{_settings.BaseUrl}/{customerCodePart}invariant/{siteMode.ToString().ToLower()}/products/{productType}?Id={string.Join("{or}", ids)}";
        }

        public string GetProductJsonDownloadUrl(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var customerCodePart = GetCustomerCodeUrlPart(customerCode);
            
            var url = $"{_settings.BaseUrl}/{customerCodePart}invariant/{siteMode.ToString().ToLower()}/products/{id}";
            if (allFields)
                fields = new[] { "*" };

            if (fields != null && fields.Any())
            {
                url += $"?fields={string.Join(",", fields)}";
            }
            return url;
        }

        private string GetProductJsonDownloadUrl(string customerCode, string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields)
        {
            var customerCodePart = GetCustomerCodeUrlPart(customerCode);
            var url = $"{_settings.BaseUrl}/{customerCodePart}invariant/{siteMode.ToString().ToLower()}/products/{slug}";
            if (parameters.HasKeys())
            {
                var queryParams = string.Join("&", parameters.AllKeys.Select(key => $"{key}={parameters[key]}"));
                url += $"?{queryParams}";
            }
            
            if (allFields)
                fields = new[] { "*" };

            if (fields != null && fields.Any())
            {
                url += parameters.HasKeys() ? "&" : "?";
                url += $"fields={string.Join(",", fields)}";
            }

            return url;
        }

        private string GetRegionTagsUrl(string customerCode, int productId, SiteMode siteMode)
        {
            var customerCodePart = GetCustomerCodeUrlPart(customerCode);
            return $"{_settings.BaseUrl}/{customerCodePart}invariant/{siteMode.ToString().ToLower()}/products/RegionTags?ProductId={productId}";
        }
        
        private string GetCustomerCodeUrlPart(string customerCode)
        {
            if (!_settings.UseConsolidatedApi)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(customerCode))
            {
                throw new CustomerCodeNotSpecifiedException("Customer code not specified");
            }

            return $"{customerCode}/";
        }

        private async Task<string> MakeRequest(string customerCode, string url)
        {
            string token;

            if (_settings.UseConsolidatedApi)
            {
                token = string.IsNullOrWhiteSpace(customerCode)
                    ? throw new CustomerCodeNotSpecifiedException("Customer code not specified")
                    : await _dpcDbClient.GetCachedHighloadApiAuthToken(customerCode);
            }
            else
            {
                token = _settings.XAuthToken;
            }

            var client = _factory.CreateClient();           
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Add("X-Auth-Token", token);
            }

            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
