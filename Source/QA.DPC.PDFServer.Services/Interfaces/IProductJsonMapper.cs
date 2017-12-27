using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IProductJsonMapper
    {
        Task<string> MapProductJson(int productId, string category, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode);

        Task<string> MapRoamingCountryJson(int? countryId, string countryCode, string category, bool isB2b, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode);
    }
}
