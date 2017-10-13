using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IRegionTagsReplacer
    {
        Task<string> ReplaceTags(string input, int productId, SiteMode siteMode, int? regionId);
    }
}
