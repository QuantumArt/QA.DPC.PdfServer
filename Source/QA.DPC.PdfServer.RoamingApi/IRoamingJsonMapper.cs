using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PdfServer.RoamingApi
{
    public interface IRoamingJsonMapper
    {
        Task<string> MapRoamingCountryJson(string customerCode, int? countryId, string countryCode, string category, bool isB2b, int? mapperId, int? templateId, bool forceDownload, SiteMode siteMode);
    }
}
