using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IProductJsonMapper
    {
        Task<string> MapProductJson(string customerCode, int productId, string category, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode);
    }
}
