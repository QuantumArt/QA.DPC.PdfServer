using System.Threading.Tasks;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;
using QA.DPC.PDFServer.Services.Interfaces;

namespace QA.DPC.PdfServer.RoamingApi
{
    public class RoamingPdfGenerationSettingsProvider : PdfGenerationSettingsProvider
    {
        public RoamingPdfGenerationSettingsProvider(IDpcApiClient dpcApiClient) : base(dpcApiClient)
        {
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