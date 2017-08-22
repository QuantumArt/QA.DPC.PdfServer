using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class HtmlGenerator: IHtmlGenerator
    {
        private readonly NodeServerSettings _settings;
        private readonly IPdfTemplateSelector _pdfTemplateSelector;
        private readonly IDpcApiClient _client;

        public HtmlGenerator(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient client)
        {
            _settings = settings.Value;
            _pdfTemplateSelector = pdfTemplateSelector;
            _client = client;
        }


        public async Task<string> GenerateHtml(int productId, string category)
        {
            var pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateId(productId, category);
            
            var productBase = await _client.GetProduct<DpcProductBase>(productId, false, new[] { "Id", "UpdateDate" });
            var productDownloadUrl = _client.GetProductJsonDownloadUrl(productId, true);
            var request = new GenerateHtmlRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = productId,
                    Timestamp = ConvertToTimestamp(productBase.UpdateDate),
                    DownloadUrl = productDownloadUrl
                },
                MapperData = new GenerateHtmlFileInfo
                {
                    Id = pdfTemplate.PdfScriptMapper.Id,
                    Timestamp = ConvertToTimestamp(pdfTemplate.PdfScriptMapper.Timestamp),
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfScriptMapper.PdfScriptMapperFile.AbsoluteUrl}"
                },
                TemplateData = new GenerateHtmlFileInfo
                {
                    Id = pdfTemplate.Id,
                    Timestamp = ConvertToTimestamp(pdfTemplate.UpdateDate),
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfTemplateFile.AbsoluteUrl}"
                },
                TemplateEngine = pdfTemplate.PdfTemplateEngine
            };
            var generatedHtmlRelativeUrl = await MakeGenerateRequest(request);
            return await GetHtml(generatedHtmlRelativeUrl);
        }

        private async Task<string> GetHtml(string generatedHtmlRelativeUrl)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync($"{_settings.GenerateBaseUrl}/output/{generatedHtmlRelativeUrl}");
            }
        }


        private async Task<string> MakeGenerateRequest(GenerateHtmlRequest request)
        {
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/generate",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                return await result.Content.ReadAsStringAsync();
            }
        }

        private static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }
    }
}
