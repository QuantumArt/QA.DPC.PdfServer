using Newtonsoft.Json;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using QA.DPC.PDFServer.Services.Exceptions;

namespace QA.DPC.PDFServer.Services
{
    public class PdfGenerationSettingsProcessor
    {
        public static string GetImpactApiBaseUrlForRoaming(PdfTemplate pdfTemplate)
        {
            if (string.IsNullOrWhiteSpace(pdfTemplate.PdfGenerationSettings?.ValueJson))
            {
                throw new RoamingPdfGenerationNotConfiguredException("Pdf generation settings not found");
            }

            var pdfGenerationSettings = JsonConvert.DeserializeObject<PdfGenerationSettings>(pdfTemplate.PdfGenerationSettings.ValueJson);

            if (!pdfGenerationSettings.RoamingGenerationEnabled)
            {
                throw new RoamingPdfGenerationNotConfiguredException("Roaming pdf generation disabled");
            }

            if (string.IsNullOrWhiteSpace(pdfGenerationSettings.ImpactApiBaseUrl))
            {
                throw new RoamingPdfGenerationNotConfiguredException("Impact api base url is not specified");
            }

            return pdfGenerationSettings.ImpactApiBaseUrl;
        }
    }
}