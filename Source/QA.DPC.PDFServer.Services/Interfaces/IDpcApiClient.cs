using System.Collections.Generic;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;
using System.Collections.Specialized;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IDpcApiClient
    {
        Task<string> GetProductJson(string customerCode, string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null);
        
        Task<string> GetProductJson(string customerCode, int id, SiteMode siteMode);
        Task<string> GetProductJson(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null);
        Task<T> GetProduct<T>(string customerCode, int id, SiteMode siteMode);
        Task<T> GetProduct<T>(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null);
        Task<T> GetProduct<T>(string customerCode, string slug, NameValueCollection parameters, bool allFields, SiteMode siteMode, string[] fields = null);

        Task<RegionTags[]> GetRegionTags(string customerCode, int productId, SiteMode siteMode);
        
       
        Task<string> GetProductsJson(string customerCode, string productType, int[] ids, SiteMode siteMode, string[] fields = null);
        Task<IEnumerable<T>> GetProducts<T>(string customerCode, string productType, int[] ids, SiteMode siteMode, string[] fields = null);
        string GetProductJsonDownloadUrl(string customerCode, int id, bool allFields, SiteMode siteMode, string[] fields = null);

    }
}
