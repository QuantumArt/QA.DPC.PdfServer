using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using QA.DPC.PdfServer.RoamingApi.Interfaces;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.DataContract.HtmlGenerator;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PdfServer.RoamingApi.Services
{
    public class RoamingHtmlGenerator : HtmlGenerator
    {

        private IImpactApiClient _impactApiClient;

        private ImpactApiSettings _impactApiSettings;
        public RoamingHtmlGenerator(IOptions<NodeServerSettings> settings, IOptions<ImpactApiSettings> impactSettings, 
            IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient client, IImpactApiClient impactApiClient, 
            IRegionTagsReplacer regionTagsReplacer, PdfGenerationSettingsProvider pdfGenerationSettingsProvider, IHttpClientFactory factory) : 
            base(settings, pdfTemplateSelector, client, regionTagsReplacer, pdfGenerationSettingsProvider, factory)
        {
            _impactApiSettings = impactSettings.Value;
            _impactApiClient = impactApiClient;
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


            var productDownloadUrl =
                _impactApiClient.GetRoamingProductDownloadUrl(_impactApiSettings.BaseUrl, cCode, isB2B, siteMode);


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

    }
}