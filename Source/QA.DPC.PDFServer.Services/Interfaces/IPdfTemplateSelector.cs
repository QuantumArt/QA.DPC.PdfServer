using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IPdfTemplateSelector
    {
        Task<PdfTemplate> GetPdfTemplate(int productId, string category, SiteMode siteMode);
    }
}
