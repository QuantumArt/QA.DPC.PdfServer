using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IPdfGenerationSettingsProvider
    {
        Task<PdfGenerationSettings> GetDefaultSettings(string customerCode, SiteMode siteMode);
        Task<PdfGenerationSettings> GetSettings(string customerCode, PdfTemplate template, SiteMode siteMode);

        Task<string> GetImpactApiBaseUrlForRoaming(string customerCode, PdfTemplate pdfTemplate, SiteMode siteMode);
    }
}