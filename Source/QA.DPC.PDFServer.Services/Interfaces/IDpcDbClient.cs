using System.Threading.Tasks;
using QP.ConfigurationService.Models;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IDpcDbClient
    {
        Task<string> GetHighloadApiAuthToken(string customerCode);
        Task<string> GetHighloadApiAuthToken(CustomerConfiguration configuration);

        Task<string> GetCachedHighloadApiAuthToken(string customerCode);
        Task<string> GetCachedHighloadApiAuthToken(CustomerConfiguration configuration);
    }
}