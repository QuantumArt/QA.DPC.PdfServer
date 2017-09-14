using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QA.DPC.PDFServer.Services.DataContract.DpcApi;

namespace QA.DPC.PDFServer.Services
{
    public interface IDpcApiClient
    {
        Task<string> GetProductJson(int id, SiteMode siteMode);
        Task<string> GetProductJson(int id, bool allFields, SiteMode siteMode, string[] fields = null);
        Task<T> GetProduct<T>(int id, SiteMode allFields);
        Task<T> GetProduct<T>(int id, bool allFields, SiteMode siteMode, string[] fields = null);

        Task<RegionTags[]> GetRegionTags(int productId, SiteMode siteMode);
       
        Task<string> GetProductsJson(string productType, int[] ids, SiteMode siteMode, string[] fields = null);
        Task<IEnumerable<T>> GetProducts<T>(string productType, int[] ids, SiteMode siteMode, string[] fields = null);
        string GetProductJsonDownloadUrl(int id, bool allFields, SiteMode siteMode, string[] fields = null);

    }
}
