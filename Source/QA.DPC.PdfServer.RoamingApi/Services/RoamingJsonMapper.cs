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
    public class RoamingJsonMapper : ProductJsonMapper, IRoamingJsonMapper
    {
        private ImpactApiSettings _impactApiSettings;
        
        public RoamingJsonMapper(
            IOptions<NodeServerSettings> settings, IOptions<ImpactApiSettings> impactSettings, IPdfTemplateSelector pdfTemplateSelector, IDpcApiClient dpcApiClient, 
            IHttpClientFactory factory, IImpactApiClient impactApiClient) 
            : base(settings, pdfTemplateSelector, dpcApiClient, factory)
        {
            _impactApiSettings = impactSettings.Value;
            _impactApiClient = impactApiClient;
        }

        private IImpactApiClient _impactApiClient;

        public async Task<string> MapRoamingCountryJson(string customerCode, int? countryId, string countryCode, string category, bool isB2b, int? mapperId,
            int? templateId, bool forceDownload, SiteMode siteMode)
        {
            string cCode = null;
            if (countryId.HasValue)
            {
                var article = await _dpcApiClient.GetProduct<RoamingCountry>(customerCode, countryId.Value, siteMode);
                if (article != null)
                {
                    cCode = article.Alias;
                }
            }
            else
            {
                cCode = countryCode;
            }


            PdfTemplate pdfTemplate = null;
            if (!mapperId.HasValue)
            {
                if (templateId.HasValue)
                {
                    pdfTemplate = await _dpcApiClient.GetProduct<PdfTemplate>(customerCode, templateId.Value, siteMode);
                }
                else
                {
                    pdfTemplate = await _pdfTemplateSelector.GetPdfTemplateForRoaming(customerCode, cCode, category, isB2b, siteMode);
                }
                mapperId = pdfTemplate.PdfScriptMapper.Id;
            }

            

            var mapper = await _dpcApiClient.GetProduct<PdfScriptMapper>(customerCode, mapperId.Value, true, siteMode);
            var productDownloadUrl = _impactApiClient.GetRoamingProductDownloadUrl(_impactApiSettings.BaseUrl, cCode, isB2b, siteMode);

            var request = new PreviewJsonRequest
            {
                TariffData = new GenerateHtmlFileInfo
                {
                    Id = $"{cCode}_{isB2b}".GetHashCode(), // не очень правильно, но в данном случае - сойдет
                    Timestamp = ConvertToTimestamp(DateTime.UtcNow),
                    ForceDownload = forceDownload,
                    DownloadUrl = productDownloadUrl,
                    SiteMode = siteMode.ToString()
                },

                MapperData = new GenerateHtmlFileInfo
                {
                    Id = mapperId.Value,
                    Timestamp = ConvertToTimestamp(mapper.Timestamp),
                    ForceDownload = forceDownload,
                    DownloadUrl = $"{_settings.DpcStaticFilesScheme}:{mapper.PdfScriptMapperFile.AbsoluteUrl}",
                    SiteMode = "db"
                },
            };

            var response = await MakeRequest(request);
            if (response.Success)
                return response.Json;

            throw new ProductMappingException(response.Error?.Message ?? "Unknown error while mapping product");

        }

    }
}