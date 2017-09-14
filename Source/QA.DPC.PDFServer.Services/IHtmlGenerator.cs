using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services
{
    public interface IHtmlGenerator
    {
        Task<string> GenerateHtml(int productId, string category, int? templateId, int? regionId, SiteMode siteMode);
    }
}
