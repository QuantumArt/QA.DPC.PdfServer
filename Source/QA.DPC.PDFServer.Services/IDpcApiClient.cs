using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public interface IDpcApiClient
    {
        Task<string> GetProductJson(int id);
        Task<string> GetProductJson(int id, bool allFields, string[] fields = null);
        Task<T> GetProduct<T>(int id);
        Task<T> GetProduct<T>(int id, bool allFields, string[] fields = null);
        Task<string> GetProductsJson(string productType, int[] ids, string[] fields = null);
        Task<IEnumerable<T>> GetProducts<T>(string productType, int[] ids, string[] fields = null);
        string GetProductJsonDownloadUrl(int id, bool allFields, string[] fields = null);

    }
}
