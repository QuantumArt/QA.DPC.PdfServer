using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;

namespace QA.DPC.PDFServer.Services
{
    public class DpcApiClient : IDpcApiClient
    {
        private readonly DpcApiSettings _settings;

        

        public DpcApiClient(IOptions<DpcApiSettings> settings)
        {
            _settings = settings.Value;
        }


        public async Task<string> GetProductJson(string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var url = GetProductJsonDownloadUrl(slug, parameters, allFields, siteMode, fields);
            try
            {
                return await MakeRequest(url);
            }
            catch (Exception ex)
            {
                throw new GetProductJsonException($"Get product json with slug = {slug}", ex);
            }
        }

        

        public async Task<string> GetProductJson(int id, SiteMode siteMode)
        {
            return await GetProductJson(id, false, siteMode);
        }

        public async Task<string> GetProductJson(int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var url = GetProductJsonDownloadUrl(id, allFields, siteMode, fields);
            try
            {
                return await MakeRequest(url);
            }
            catch (Exception ex)
            {
                throw new GetProductJsonException($"Get product json with id = {id} error.", ex);
            }
        }

        public async Task<T> GetProduct<T>(int id, SiteMode siteMode)
        {
            return await GetProduct<T>(id, false, siteMode);
        }

        public async Task<T> GetProduct<T>(int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var productJson = await GetProductJson(id, allFields, siteMode, fields);
            return JsonConvert.DeserializeObject<T>(productJson);
        }

        public async Task<T> GetProduct<T>(string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var productJson = await GetProductJson(slug, parameters, allFields, siteMode, fields);
            return JsonConvert.DeserializeObject<T>(productJson);
        }


        public async Task<RegionTags[]> GetRegionTags(int productId, SiteMode siteMode)
        {
            var url = $"{_settings.BaseUrl}/{siteMode.ToString().ToLower()}/products/RegionTags?ProductId={productId}";
            var json = await MakeRequest(url);
            return JsonConvert.DeserializeObject<RegionTags[]>(json);
        }


        public async Task<string> GetProductsJson(string productType, int[] ids, SiteMode siteMode, string[] fields = null)
        {
            //вариант для получения одним запросом(когда починят).
            var url = $"{_settings.BaseUrl}/{siteMode.ToString().ToLower()}/products/{productType}?Id={string.Join("{or}", ids)}";

            if (fields != null && fields.Any())
            {
                url += $"&fields={string.Join(",", fields)}";
            }

            return await MakeRequest(url);
        }

        public async Task<IEnumerable<T>> GetProducts<T>(string productType, int[] ids, SiteMode siteMode, string[] fields = null)
        {
            var productsJsons = await GetProductsJson(productType, ids, siteMode, fields);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(productsJsons);
        }

        public string GetProductJsonDownloadUrl(int id, bool allFields, SiteMode siteMode, string[] fields = null)
        {
            var url = $"{_settings.BaseUrl}/{siteMode.ToString().ToLower()}/products/{id}";
            if (allFields)
                fields = new[] { "*" };

            if (fields != null && fields.Any())
            {
                url += $"?fields={string.Join(",", fields)}";
            }
            return url;
        }

        private string GetProductJsonDownloadUrl(string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields)
        {
            var url = $"{_settings.BaseUrl}/{siteMode.ToString().ToLower()}/products/{slug}";
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


        private async Task<string> MakeRequest(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", _settings.XAuthToken);
                return await client.GetStringAsync(url);
            }
            
        }
    }
}
