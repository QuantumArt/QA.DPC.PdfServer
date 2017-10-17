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
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services
{
    public class HtmlGenerator: IHtmlGenerator
    {
        private readonly NodeServerSettings _settings;
        private readonly IPdfTemplateSelector _pdfTemplateSelector;
        private readonly IDpcApiClient _client;
        private readonly IImpactApiClient _impactApiClient;
        private readonly IRegionTagsReplacer _regionTagsReplacer;

        public HtmlGenerator(IOptions<NodeServerSettings> settings, IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient client, IImpactApiClient impactApiClient, IRegionTagsReplacer regionTagsReplacer)
        {
            _settings = settings.Value;
            _pdfTemplateSelector = pdfTemplateSelector;
            _client = client;
            _impactApiClient = impactApiClient;
            _regionTagsReplacer = regionTagsReplacer;
        }


        

        public async Task<string> GenerateHtml(int productId, string category, int? templateId, int? regionId, SiteMode siteMode, bool forceDownload)
        {
            PdfTemplate pdfTemplate;
            if (templateId.HasValue)
            {
                pdfTemplate = await _client.GetProduct<PdfTemplate>(templateId.Value, siteMode);
            }
            else
            {
                pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateForProduct(productId, category, siteMode);
            }
            
            if(pdfTemplate == null)
                throw new TemplateNotFoundException();
            
            var productBase = await _client.GetProduct<DpcProductBase>(productId, false, siteMode, new[] { "Id", "UpdateDate" });
            if (productBase == null)
                throw new GetProductJsonException();
            var productDownloadUrl = _client.GetProductJsonDownloadUrl(productId, true, siteMode);
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
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfScriptMapper.PdfScriptMapperFile.AbsoluteUrl}",
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
                var generateHtml = await GetHtml(response.RelativePath);
                var replacedHtml = await _regionTagsReplacer.ReplaceTags(generateHtml, productId, siteMode, regionId);
                return replacedHtml;
            }

            throw new HtmlGenerationException(response.Error?.Message ?? "Unknown error while generating html");
        }

        public async Task<string> GenerateRoamingHtml(string category, string countryCode, bool isB2B, int? templateId, SiteMode siteMode, bool forceDownload)
        {
            PdfTemplate pdfTemplate;
            if (templateId.HasValue)
            {
                pdfTemplate = await _client.GetProduct<PdfTemplate>(templateId.Value, siteMode);
            }
            else
            {
                pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateForRoaming(countryCode, category, isB2B, siteMode);
            }

            if (pdfTemplate == null)
                throw new TemplateNotFoundException();

            var productDownloadUrl = _impactApiClient.GetRoamingProductDownloadUrl(countryCode, isB2B, siteMode);


            var request = new GenerateHtmlRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = $"{countryCode}_{isB2B}".GetHashCode(), // не очень правильно, но в данном случае - сойдет
                    Timestamp = ConvertToTimestamp(DateTime.UtcNow),
                    DownloadUrl = productDownloadUrl,
                    ForceDownload = forceDownload,
                    SiteMode = siteMode.ToString()
                },
                MapperData = new GenerateHtmlFileInfo
                {
                    Id = pdfTemplate.PdfScriptMapper.Id,
                    Timestamp = ConvertToTimestamp(pdfTemplate.PdfScriptMapper.Timestamp),
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{pdfTemplate.PdfScriptMapper.PdfScriptMapperFile.AbsoluteUrl}",
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
