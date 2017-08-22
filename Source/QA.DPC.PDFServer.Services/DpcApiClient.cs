using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace QA.DPC.PDFServer.Services
{
    public class DpcApiClient : IDpcApiClient
    {
        private readonly DpcApiSettings _settings;

        

        public DpcApiClient(IOptions<DpcApiSettings> settings)
        {
            _settings = settings.Value;
        }


        public async Task<string> GetProductJson(int id)
        {
            return await GetProductJson(id, false);
        }

        public async Task<string> GetProductJson(int id, bool allFields, string[] fields = null)
        {
            var url = GetProductJsonDownloadUrl(id, allFields, fields);
            return await MakeRequest(url);
        }

        public async Task<T> GetProduct<T>(int id)
        {
            return await GetProduct<T>(id, false);
        }

        public async Task<T> GetProduct<T>(int id, bool allFields, string[] fields = null)
        {
            var productJson = await GetProductJson(id, allFields, fields);
            return JsonConvert.DeserializeObject<T>(productJson);
        }

        public async Task<string> GetProductsJson(string productType, int[] ids, string[] fields = null)
        {
           

            //вариант для получения одним запросом(когда починят).
            var url = $"{_settings.BaseUrl}/products/{productType}?Id={string.Join("{or}", ids)}";

            if (fields != null && fields.Any())
            {
                url += $"&fields={string.Join(",", fields)}";
            }

            return await MakeRequest(url);
        }

        public async Task<IEnumerable<T>> GetProducts<T>(string productType, int[] ids, string[] fields = null)
        {
            var productsJsons = await GetProductsJson(productType, ids, fields);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(productsJsons);
        }

        public string GetProductJsonDownloadUrl(int id, bool allFields, string[] fields = null)
        {
            var url = $"{_settings.BaseUrl}/products/{id}";
            if (allFields)
                fields = new[] { "*" };

            if (fields != null && fields.Any())
            {
                url += $"?fields={string.Join(",", fields)}";
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
