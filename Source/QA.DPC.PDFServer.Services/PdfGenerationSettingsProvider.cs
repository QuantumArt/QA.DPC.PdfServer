using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;

namespace QA.DPC.PDFServer.Services
{
    public class PdfGenerationSettingsProvider : IPdfGenerationSettingsProvider
    {
        private readonly IDpcApiClient _dpcApiClient;

        public PdfGenerationSettingsProvider(IDpcApiClient dpcApiClient)
        {
            _dpcApiClient = dpcApiClient;
        }

        public async Task<PdfGenerationSettings> GetDefaultSettings(string customerCode, SiteMode siteMode)
        {
            var settingsArray = await _dpcApiClient.GetProduct<PdfGenerationSettingsProduct[]>(customerCode, "pdfgenerationsettings",
                new NameValueCollection
                {
                    {"IsDefault", "true"}
                }, true, siteMode);
            var settings = settingsArray?.FirstOrDefault();
            return string.IsNullOrWhiteSpace(settings?.ValueJson) 
                ? null 
                : JsonConvert.DeserializeObject<PdfGenerationSettings>(settings.ValueJson);
        }

        public async Task<PdfGenerationSettings> GetSettings(string customerCode, PdfTemplate template, SiteMode siteMode)
        {
            return string.IsNullOrWhiteSpace(template?.PdfGenerationSettings?.ValueJson)
                ? await GetDefaultSettings(customerCode, siteMode)
                : JsonConvert.DeserializeObject<PdfGenerationSettings>(template.PdfGenerationSettings.ValueJson);
        }

        public async Task<string> GetImpactApiBaseUrlForRoaming(string customerCode, PdfTemplate pdfTemplate, SiteMode siteMode)
        {
            var settings = await GetSettings(customerCode, pdfTemplate, siteMode);
            
            if (settings == null)
            {
                throw new RoamingPdfGenerationNotConfiguredException("Pdf generation settings not found");
            }

            if (!settings.RoamingGenerationEnabled)
            {
                throw new RoamingPdfGenerationNotConfiguredException("Roaming pdf generation disabled");
            }

            if (string.IsNullOrWhiteSpace(settings.ImpactApiBaseUrl))
            {
                throw new RoamingPdfGenerationNotConfiguredException("Impact api base url is not specified");
            }

            return settings.ImpactApiBaseUrl;
        }
    }
}