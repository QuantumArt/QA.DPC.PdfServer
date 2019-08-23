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
        private readonly NodeServerSettings _settings;
        private readonly IPdfTemplateSelector _pdfTemplateSelector;
        private readonly IDpcApiClient _client;
        private readonly IImpactApiClient _impactApiClient;
        private readonly IRegionTagsReplacer _regionTagsReplacer;
        private readonly IPdfGenerationSettingsProvider _pdfGenerationSettingsProvider;
        private readonly IHttpClientFactory _factory;

        public HtmlGenerator(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector,
            IDpcApiClient client, IImpactApiClient impactApiClient, IRegionTagsReplacer regionTagsReplacer,
            IPdfGenerationSettingsProvider pdfGenerationSettingsProvider, IHttpClientFactory factory)
        {
            _settings = settings.Value;
            _pdfTemplateSelector = pdfTemplateSelector;
            _client = client;
            _impactApiClient = impactApiClient;
            _regionTagsReplacer = regionTagsReplacer;
            _pdfGenerationSettingsProvider = pdfGenerationSettingsProvider;
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

        public async Task<string> GenerateRoamingHtml(string customerCode, string category, int? roamingCountryId,
            string countryCode, bool isB2B, int? templateId, SiteMode siteMode, bool forceDownload)
        {
            string cCode = null;
            if (roamingCountryId.HasValue)
            {
                var article = await _client.GetProduct<RoamingCountry>(customerCode, roamingCountryId.Value, siteMode);
                if (article != null)
                {
                    cCode = article.Alias;
                }
            }
            else
            {
                cCode = countryCode;
            }

            PdfTemplate pdfTemplate;
            if (templateId.HasValue)
            {
                pdfTemplate = await _client.GetProduct<PdfTemplate>(customerCode, templateId.Value, siteMode);
            }
            else
            {
                pdfTemplate =
                    await _pdfTemplateSelector.GetPdfTemplateForRoaming(customerCode, cCode, category, isB2B, siteMode);
            }

            if (pdfTemplate == null)
                throw new TemplateNotFoundException();


            var impactApiBaseUrl = await _pdfGenerationSettingsProvider.GetImpactApiBaseUrlForRoaming(customerCode, pdfTemplate, siteMode);

            var productDownloadUrl =
                _impactApiClient.GetRoamingProductDownloadUrl(impactApiBaseUrl, cCode, isB2B, siteMode);


            var request = new GenerateHtmlRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = $"{cCode}_{isB2B}".GetHashCode(), // не очень правильно, но в данном случае - сойдет
                    Timestamp = ConvertToTimestamp(DateTime.UtcNow),
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
                return await GetHtml(response.RelativePath);
            }

            throw new HtmlGenerationException(response.Error?.Message ?? "Unknown error while generating html");
        }

        private async Task<string> GetHtml(string generatedHtmlRelativeUrl)
        {
            var client = _factory.CreateClient();
            return await client.GetStringAsync($"{_settings.OutputBaseUrl}/{generatedHtmlRelativeUrl}");
        }


        private async Task<GenerateHtmlResponse> MakeGenerateRequest(GenerateHtmlRequest request)
        {
            var client = _factory.CreateClient();
            var result = await client.PostAsync($"{_settings.GenerateBaseUrl}/generate",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GenerateHtmlResponse>(stringResult);
        }

        private static long ConvertToTimestamp(DateTime date)
        {
            return ((DateTimeOffset) date).ToUnixTimeSeconds();
        }
    }
}