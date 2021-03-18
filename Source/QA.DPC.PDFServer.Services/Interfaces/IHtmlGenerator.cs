using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IHtmlGenerator
    {
        Task<string> GenerateHtml(string customerCode, int productId, string category, int? templateId, int? regionId, SiteMode siteMode, bool forceDownload);

    }
}
