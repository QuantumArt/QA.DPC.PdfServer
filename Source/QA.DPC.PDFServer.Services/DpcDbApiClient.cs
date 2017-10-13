using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class DpcDbApiClient : IDpcDbApiClient
    {
        private readonly DpcDbApiSettings _settings;

        public DpcDbApiClient(IOptions<DpcDbApiSettings> settings)
        {
            _settings = settings.Value;
        }


        public async Task<T> GetProduct<T>(int id, string serviceSlug)
        {
            var productJson = await GetProductJson(id, serviceSlug);
            return JsonConvert.DeserializeObject<T>(productJson);
        }

        public async Task<PdfScriptMapper> GetPdfScriptMapper(int id)
        {
            var product = await GetProduct<DbApiProductWrapper<PdfScriptMapper>>(id, "PdfScriptMappers");
            return product.Product;
        }

        private async Task<string> GetProductJson(int id, string serviceSlug)
        {
            var url = GetProductJsonDownloadUrl(id, serviceSlug);
            try
            {
                return await MakeRequest(url);
            }
            catch (Exception ex)
            {
                throw new GetProductJsonException($"Get product json with id = {id} error.", ex);
            }
        }

        private static async Task<string> MakeRequest(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        private string GetProductJsonDownloadUrl(int id, string serviceSlug)
        {
            return $"{_settings.BaseUrl}/v1/{serviceSlug}/json/{id}";
        }
    }
}
