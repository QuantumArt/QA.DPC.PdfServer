using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class HtmlGenerator : IHtmlGenerator
    {
        protected readonly NodeServerSettings _settings;
        protected readonly IPdfTemplateSelector _pdfTemplateSelector;
        protected readonly IDpcApiClient _client;
        private readonly IRegionTagsReplacer _regionTagsReplacer;
        private readonly IPdfGenerationSettingsProvider _roamingPdfGenerationSettingsProvider;
        private readonly IHttpClientFactory _factory;

        public HtmlGenerator(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector,
            IDpcApiClient client, IRegionTagsReplacer regionTagsReplacer,
            IPdfGenerationSettingsProvider roamingPdfGenerationSettingsProvider, IHttpClientFactory factory)
        {
            _settings = settings.Value;
            _pdfTemplateSelector = pdfTemplateSelector;
            _client = client;
            _regionTagsReplacer = regionTagsReplacer;
            _roamingPdfGenerationSettingsProvider = roamingPdfGenerationSettingsProvider;
            _factory = factory;
        }


        public async Task<string> GenerateHtml(string customerCode, int productId, string category, int? templateId,
            int? regionId, SiteMode siteMode, bool forceDownload)
        {
            PdfTemplate pdfTemplate;
            if (templateId.HasValue)
            {
                pdfTemplate = await _client.GetProduct<PdfTemplate>(customerCode, templateId.Value, siteMode);
            }
            else
            {
                pdfTemplate =
                    await _pdfTemplateSelector.GetPdfTemplateForProduct(customerCode, productId, category, siteMode);
            }

            if (pdfTemplate == null)
                throw new TemplateNotFoundException();

            var productBase = await _client.GetProduct<DpcProductBase>(customerCode, productId, false, siteMode,
                new[] {"Id", "UpdateDate"});
            if (productBase == null)
                throw new GetProductJsonException();
            var productDownloadUrl = _client.GetProductJsonDownloadUrl(customerCode, productId, true, siteMode);
            var request = new GenerateHtmlRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = productId,
                    Timestamp = ConvertToTimestamp(productBase.UpdateDate),
                    DownloadUrl = productDownloadUrl,
                    ForceDownload = forceDownload,
                    SiteMode = siteMode.ToString()
                },
                MapperData = new GenerateHtmlFileInfo
                {
                    Id = pdfTemplate.PdfScriptMapper.Id,
                    Timestamp = ConvertToTimestamp(pdfTemplate.PdfScriptMapper.Timestamp),
                    DownloadUrl =
                        $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfScriptMapper.PdfScriptMapperFile.AbsoluteUrl}",
                    ForceDownload = forceDownload,
                    SiteMode = siteMode.ToString()
                },
                TemplateData = new GenerateHtmlFileInfo
                {
                    Id = pdfTemplate.Id,
                    Timestamp = ConvertToTimestamp(pdfTemplate.UpdateDate),
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfTemplateFile.AbsoluteUrl}",
                    ForceDownload = forceDownload,
                    SiteMode = siteMode.ToString()
                },
                TemplateEngine = pdfTemplate.PdfTemplateEngine
            };
            var response = await MakeGenerateRequest(request);
            if (response.Success && !string.IsNullOrWhiteSpace(response.RelativePath))
            {
                var generatedHtml = await GetHtml(response.RelativePath);
                var replacedHtml =
                    await _regionTagsReplacer.ReplaceTags(customerCode, generatedHtml, productId, siteMode, regionId);
                return replacedHtml;
            }

            throw new HtmlGenerationException(response.Error?.Message ?? "Unknown error while generating html");
        }

        protected async Task<string> GetHtml(string generatedHtmlRelativeUrl)
        {
            var client = _factory.CreateClient();
            if (!String.IsNullOrEmpty(_settings.OutputBaseUrl))
            {
                return await client.GetStringAsync($"{_settings.OutputBaseUrl}/{generatedHtmlRelativeUrl}");
            }
            else
            {
                return await client.GetStringAsync($"{_settings.GenerateBaseUrl}/{generatedHtmlRelativeUrl}");
            }
        }


        protected async Task<GenerateHtmlResponse> MakeGenerateRequest(GenerateHtmlRequest request)
        {
            var client = _factory.CreateClient();
            var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/generate",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GenerateHtmlResponse>(stringResult);
        }

        protected static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset) date).ToUnixTimeSeconds();
        }
    }
}