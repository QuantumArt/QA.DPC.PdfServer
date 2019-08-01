using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IPdfTemplateSelector
    {
        Task<PdfTemplate> GetPdfTemplateForProduct(string customerCode, int productId, string category, SiteMode siteMode);
        Task<PdfTemplate> GetPdfTemplateForRoaming(string customerCode, string countryCode, string category, bool isB2bB, SiteMode siteMode);
    }
}
