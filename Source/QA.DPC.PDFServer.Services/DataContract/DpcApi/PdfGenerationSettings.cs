using QA.DPC.PDFServer.Services.Settings;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class PdfGenerationSettings
    {
        public bool RoamingGenerationEnabled { get; set; }
        public string ImpactApiBaseUrl { get; set; }
        public PdfTemplateSelectorSettings PdfTemplateSelector { get; set; }
    }
}