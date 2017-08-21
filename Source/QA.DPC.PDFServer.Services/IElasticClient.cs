using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services
{
    public interface IElasticClient
    {
        Task<string> GetProductJson(int id);
        Task<string> GetProductJson(int id, bool allFields, string[] fields = null);
        
    }
}
