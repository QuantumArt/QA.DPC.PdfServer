using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
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
            var pdfTemplate = await _pdfTemplateSelector.GetPdfTemplate(productId, category);
            if(pdfTemplate == null)
                throw new TemplateNotFoundException();
            
            var productBase = await _client.GetProduct<DpcProductBase>(productId, false, new[] { "Id", "UpdateDate" });
            if (productBase == null)
                throw new GetProductJsonException();
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
            var response = await MakeGenerateRequest(request);
            if (response.Success && !string.IsNullOrWhiteSpace(response.RelativePath))
                return await GetHtml(response.RelativePath);

            throw new HtmlGenerationException(response.Error?.Message ?? "Unknown error while generating html");
        }

        private async Task<string> GetHtml(string generatedHtmlRelativeUrl)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync($"{_settings.GenerateBaseUrl}/output/{generatedHtmlRelativeUrl}");
            }
        }


        private async Task<GenerateHtmlResponse> MakeGenerateRequest(GenerateHtmlRequest request)
        {
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/generate",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                var stringResult = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GenerateHtmlResponse>(stringResult);
            }
        }

        private static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }
    }
}
