using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Interfaces
{
    public interface IDpcDbClient
    {
        Task<string> GetHighloadApiAuthToken(string customerCode);
        Task<string> GetHighloadApiAuthToken(CustomerCodeConfiguration configuration);

        Task<string> GetCachedHighloadApiAuthToken(string customerCode);
        Task<string> GetCachedHighloadApiAuthToken(CustomerCodeConfiguration configuration);
    }
}