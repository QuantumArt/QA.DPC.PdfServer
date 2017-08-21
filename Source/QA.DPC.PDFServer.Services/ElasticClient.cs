using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.Settings;
using Microsoft.Extensions.Options;

namespace QA.DPC.PDFServer.Services
{
    public class ElasticClient : IElasticClient
    {
        private readonly ElasticSettings _settings;

        

        public ElasticClient(IOptions<ElasticSettings> settings)
        {
            _settings = settings.Value;
        }


        public Task<string> GetProductJson(int id)
        {
            return GetProductJson(id, false);
        }

        public async Task<string> GetProductJson(int id, bool allFields, string[] fields = null)
        {
            var url = $"{_settings.BaseUrl}/products/{id}";
            if (allFields)
                fields = new[] {"*"};

            if(fields != null && fields.Any())
            {
                url += $"?fields={string.Join(",", fields)}";
            }

            return await MakeRequest(url);
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
